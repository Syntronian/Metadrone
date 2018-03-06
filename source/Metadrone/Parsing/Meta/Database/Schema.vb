﻿Imports Metadrone.Parser.Syntax.Strings
Imports Metadrone.Parser.Syntax.Constants
Imports Metadrone.Parser.Source
Imports Metadrone.PluginInterface.Sources

Namespace Parser.Meta.Database

    Friend Class Schema
        Implements IEntityConnection

        Private Connection As IConnection

        Private UseOnlyList As New List(Of String)
        Private IgnoreList As New List(Of String)

        Private Tables As New List(Of IEntity)
        Private FilteredTables As New List(Of IEntity)
        Private FilteredViews As New List(Of IEntity)

        Private Routines As New List(Of IEntity)
        Private FilteredProcedures As New List(Of IEntity)
        Private FilteredFunctions As New List(Of IEntity)

        Private Transforms As Syntax.SourceTransforms = Nothing


        Public Sub New(ByVal Source As Parser.Source.Source)
            'Set up connection
            Me.Connection = ConnectionFactory.EvaluateProvider(Source)

            'Just abort on failed evaluation
            If TypeOf Me.Connection Is NullConnection Then
                Throw New Exception("Failed to evalute connection provider.")
            End If

            'Transform instructions
            Me.Transforms = Source.Transforms
        End Sub

        Public Function GetName() As String Implements IEntityConnection.GetName
            Return Me.Connection.Name
        End Function

        Public Function GetConnectionString() As String Implements IEntityConnection.GetConnectionString
            Return Me.Connection.ConnectionString
        End Function

        Friend Sub ApplyUseOnly(ByVal Name As String)
            'Add to list
            Me.UseOnlyList.Add(Name)

            Call Me.InitEntities()
        End Sub

        Friend Sub ApplyIgnore(ByVal Name As String)
            'Add to list
            Me.IgnoreList.Add(Name)

            Call Me.InitEntities()
        End Sub

        Public Sub LoadSchema() Implements IEntityConnection.LoadSchema
            Dim Schema As New List(Of SchemaRow)
            Try
                Schema = Me.Connection.GetSchema()
            Catch ex As Exception
                'Indicate as a schema get exception
                Throw New Exception("Load schema exception: " & ex.Message)
            End Try

            Me.Tables = New List(Of IEntity)
            Me.FilteredTables = New List(Of IEntity)
            Me.FilteredViews = New List(Of IEntity)

            Dim ListPos As Integer = 0
            Dim idx As Integer = 0
            While idx < Schema.Count
                'Build table/view
                Dim currIdx As Integer = idx
                Dim Table As New Table(Schema, Me, Me.Connection, Me.Transforms, idx)

                'Add to table collection, if not in ignore list
                Dim Ignore As Boolean = False
                For Each ignoreTable In Me.Connection.IgnoreTableNames
                    If StrEq(Table.Value.ToString, ignoreTable) Then
                        Ignore = True
                        Exit For
                    End If
                Next
                If Not Ignore Then
                    ListPos += 1
                    Table.ListPos = ListPos
                    Me.Tables.Add(Table)
                End If

                'Prevent infinite loop
                If idx = currIdx Then idx += 1
            End While

            'Set listcount
            For Each tbl In Me.Tables
                CType(tbl, Table).ListCount = Me.Tables.Count
            Next
        End Sub

        Public Sub LoadRoutineSchema() Implements IEntityConnection.LoadRoutineSchema
            Dim ParamSchema As New List(Of SchemaRow)
            Try
                ParamSchema = Me.Connection.GetRoutineSchema()
            Catch ex As Exception
                'Indicate as a routine schema get exception
                Throw New Exception("Load routine schema exception: " & ex.Message)
            End Try

            Me.Routines = New List(Of IEntity)
            Me.FilteredProcedures = New List(Of IEntity)
            Me.FilteredFunctions = New List(Of IEntity)

            Dim ListPos As Integer = 0
            Dim idx As Integer = 0
            While idx < ParamSchema.Count
                'Build table/view
                Dim currIdx As Integer = idx
                Dim Routine As New Routine(ParamSchema, Me, Me.Connection, Me.Transforms, idx)

                'Add to table collection, if not in ignore list
                Dim Ignore As Boolean = False
                For Each ignoreTable In Me.Connection.IgnoreTableNames
                    If StrEq(Routine.Value.ToString, ignoreTable) Then
                        Ignore = True
                        Exit For
                    End If
                Next
                If Not Ignore Then
                    'Set listpos
                    ListPos += 1
                    Routine.ListPos = ListPos

                    'Set up param list
                    Dim paramList As New List(Of String)
                    For Each p In Routine.Params
                        paramList.Add(p.GetAttributeValue("", Nothing, True, False).ToString)
                    Next

                    'Get column schema from the routine
                    Try
                        Dim ColumnSchema As New List(Of SchemaRow)
                        ColumnSchema = Me.Connection.GetRoutineColumnSchema(Routine.SchemaRowVal.Name, _
                                                                            Routine.SchemaRowVal.Type, _
                                                                            Routine.SchemaRowVal.IsProcedure, _
                                                                            paramList)
                        Call Routine.AddColumns(ColumnSchema, Me.Connection)
                    Catch ex As Exception
                        'Ignore exceptions, except when debugging in non-thread mode
                        If My.Application.EXEC_ErrorSensitivity > My.MyApplication.ErrorSensitivity.Standard Then Throw

                    End Try

                    'Finally add
                    Me.Routines.Add(Routine)
                End If

                'Prevent infinite loop
                If idx = currIdx Then idx += 1
            End While

            'Set listcount
            For Each rtn In Me.Routines
                CType(rtn, Routine).ListCount = Me.Routines.Count
            Next
        End Sub

        Public Sub InitEntities() Implements IEntityConnection.InitEntities
            Dim UseIdx As New List(Of Integer)

            UseIdx = Me.InitEntities(Me.Tables)

            'Rebuild lists from use indexes
            Me.FilteredTables = New List(Of IEntity)
            Me.FilteredViews = New List(Of IEntity)

            For i As Integer = 0 To UseIdx.Count - 1
                With CType(Me.Tables(UseIdx(i)), Table)
                    If .IsTypeTable Then
                        Me.FilteredTables.Add(Me.Tables(UseIdx(i)))
                    ElseIf .IsTypeView Then
                        Me.FilteredViews.Add(Me.Tables(UseIdx(i)))
                    End If
                End With
            Next
            For i As Integer = 0 To Me.FilteredTables.Count - 1
                CType(Me.FilteredTables(i), Table).ListPos = i + 1
                CType(Me.FilteredTables(i), Table).ListCount = Me.FilteredTables.Count
                CType(Me.FilteredTables(i), Table).ColumnCount = CType(Me.FilteredTables(i), Table).Columns.Count
                CType(Me.FilteredTables(i), Table).IDColumnCount = 0
                CType(Me.FilteredTables(i), Table).PKColumnCount = 0
                CType(Me.FilteredTables(i), Table).FKColumnCount = 0
                For j As Integer = 0 To CType(Me.FilteredTables(i), Table).ColumnCount - 1
                    With CType(CType(Me.FilteredTables(i), Table).Columns(j), Column)
                        If .IsIdentity Then CType(Me.FilteredTables(i), Table).IDColumnCount += 1
                        If .IsPrimaryKey Then CType(Me.FilteredTables(i), Table).PKColumnCount += 1
                        If .IsForeignKey Then CType(Me.FilteredTables(i), Table).FKColumnCount += 1
                    End With
                Next
            Next
            For i As Integer = 0 To Me.FilteredViews.Count - 1
                CType(Me.FilteredViews(i), Table).ListPos = i + 1
                CType(Me.FilteredViews(i), Table).ListCount = Me.FilteredViews.Count
                CType(Me.FilteredViews(i), Table).ColumnCount = CType(Me.FilteredViews(i), Table).Columns.Count
                CType(Me.FilteredViews(i), Table).IDColumnCount = 0
                CType(Me.FilteredViews(i), Table).PKColumnCount = 0
                CType(Me.FilteredViews(i), Table).FKColumnCount = 0
                For j As Integer = 0 To CType(Me.FilteredViews(i), Table).ColumnCount - 1
                    With CType(CType(Me.FilteredViews(i), Table).Columns(j), Column)
                        If .IsIdentity Then CType(Me.FilteredViews(i), Table).IDColumnCount += 1
                        If .IsPrimaryKey Then CType(Me.FilteredViews(i), Table).PKColumnCount += 1
                        If .IsForeignKey Then CType(Me.FilteredViews(i), Table).FKColumnCount += 1
                    End With
                Next
            Next


            UseIdx = Me.InitEntities(Me.Routines)

            'Rebuild lists from use indexes
            Me.FilteredProcedures = New List(Of IEntity)
            Me.FilteredFunctions = New List(Of IEntity)

            For i As Integer = 0 To UseIdx.Count - 1
                Me.Routines(UseIdx(i)).InitEntities()
                With CType(Me.Routines(UseIdx(i)), Routine)
                    If .IsTypeProcedure Then
                        Me.FilteredProcedures.Add(Me.Routines(UseIdx(i)))
                    ElseIf .IsTypeFunction Then
                        Me.FilteredFunctions.Add(Me.Routines(UseIdx(i)))
                    End If
                End With
            Next
            For i As Integer = 0 To Me.FilteredProcedures.Count - 1
                CType(Me.FilteredProcedures(i), Routine).ListPos = i + 1
                CType(Me.FilteredProcedures(i), Routine).ListCount = Me.FilteredProcedures.Count
            Next
            For i As Integer = 0 To Me.FilteredFunctions.Count - 1
                CType(Me.FilteredFunctions(i), Routine).ListPos = i + 1
                CType(Me.FilteredFunctions(i), Routine).ListCount = Me.FilteredFunctions.Count
            Next
        End Sub

        Private Function InitEntities(ByRef EntityList As List(Of IEntity)) As List(Of Integer)
            'Build use indexes
            Dim UseIdx As New List(Of Integer)
            'From use only list
            For i As Integer = 0 To EntityList.Count - 1
                If Me.UseOnlyList.Count > 0 Then
                    For j As Integer = 0 To Me.UseOnlyList.Count - 1
                        If PackageBuilder.PreProc.IgnoreCase Then
                            If StrEq(EntityList(i).GetAttributeValue("", Nothing, True, False).ToString, Me.UseOnlyList(j).ToString) Then
                                UseIdx.Add(i)
                            End If
                        Else
                            If EntityList(i).GetAttributeValue("", Nothing, True, False).ToString.Equals(Me.UseOnlyList(j).ToString) Then
                                UseIdx.Add(i)
                            End If
                        End If
                    Next
                Else
                    UseIdx.Add(i)
                End If
            Next
            'Remove any from ignore list
            For i As Integer = 0 To Me.IgnoreList.Count - 1
                Dim removed As Boolean = True
                While removed
                    removed = False
                    For j As Integer = 0 To UseIdx.Count - 1
                        If PackageBuilder.PreProc.IgnoreCase Then
                            If StrEq(EntityList(UseIdx(j)).GetAttributeValue("", Nothing, True, False).ToString, Me.IgnoreList(i).ToString) Then
                                UseIdx.RemoveAt(j)
                                removed = True
                                Exit For
                            End If
                        Else
                            If EntityList(UseIdx(j)).GetAttributeValue("", Nothing, True, False).ToString.Equals(Me.IgnoreList(i).ToString) Then
                                UseIdx.RemoveAt(j)
                                removed = True
                                Exit For
                            End If
                        End If
                    Next
                End While
            Next

            Return UseIdx
        End Function

        Public Function GetEntities(ByVal Entity As Syntax.SyntaxNode.ExecForEntities) As List(Of IEntity) Implements IEntityConnection.GetEntities
            'Check predicate against type of entity, and return appropriate collection
            Select Case Entity
                Case Syntax.SyntaxNode.ExecForEntities.OBJECT_TABLE
                    Return Me.FilteredTables

                Case Syntax.SyntaxNode.ExecForEntities.OBJECT_VIEW
                    Return Me.FilteredViews

                Case Syntax.SyntaxNode.ExecForEntities.OBJECT_PROCEDURE
                    Return Me.FilteredProcedures

                Case Syntax.SyntaxNode.ExecForEntities.OBJECT_FUNCTION
                    Return Me.FilteredFunctions

                Case Else
                    Throw New Exception("Invalid entity. Try recompiling.")

            End Select
        End Function

        Public Sub RunScriptFile(ByVal ScriptFile As String) Implements IEntityConnection.RunScriptFile
            Call Me.Connection.RunScriptFile(ScriptFile)
        End Sub

    End Class

End Namespace