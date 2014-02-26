Imports System.Collections.Generic
Imports System.Linq

Namespace SQL
''' <summary>
'''     SomeField CRITERIA_OPERATION Value CRITERIA_LOGIC
''' </summary>
''' <remarks></remarks>
	Public Class CriteriaBuilder
		Private _list As New List(Of CriteriaItem)

#Region " Add "

		Public Sub Add(ByVal tag As String, ByVal field As String, ByVal operation As CriteriaOperation, ByVal value As Object,
					   Optional ByVal logic As CriteriaLogic = CriteriaLogic.OR)
			If operation = CriteriaOperation.BETWEEN Then
				Dim item As New CriteriaItem
				item.Tag = tag
				item.Criteria = field + " " + WrapValueByOperation(operation, value)
				item.Logic = logic
				_list.Add(item)
			Else
				'check if value have multiple keywords
				If value.GetType.ToString = "System.String" Then
					For Each foundValue As String In value.Split(" ")
						Dim item As New CriteriaItem
						item.Tag = tag
						item.Criteria = field + " " + WrapValueByOperation(operation, foundValue)
						item.Logic = logic
						_list.Add(item)
					Next
				Else
					Dim item As New CriteriaItem
					item.Tag = tag
					item.Criteria = field + " " + WrapValueByOperation(operation, value)
					item.Logic = logic
					_list.Add(item)
				End If

			End If
		End Sub

#End Region

#Region " WrapValueByOperation "

		Private Function WrapValueByOperation(ByVal operation As CriteriaOperation, ByVal value As Object) As String
			Dim ret As String = ""
			'Debug.WriteLine(value.GetType.ToString)

			Select Case operation
				Case CriteriaOperation.LIKE
					If IsNumeric(value) = False And IsDate(value) = False Then
						value = value.Replace("'", "''")
					End If

					ret = " LIKE '%" + value + "%'"

				Case CriteriaOperation.NOT_LIKE
					If IsNumeric(value) = False And IsDate(value) = False Then
						value = value.Replace("'", "''")
					End If

					ret = " NOT LIKE '%" + value + "%'"

				Case CriteriaOperation.EQUAL
					If IsNumeric(value) = False And IsDate(value) = False Then
						value = "'" + value.Replace("'", "''") + "'"
					End If

					If IsDate(value) Then
						ret = "='" + value + "'"
					Else
						If value.GetType.ToString = "System.Boolean" Then
							ret = "='" + value.ToString + "'"
						Else
							ret = "=" + value.ToString
						End If
					End If

				Case CriteriaOperation.NOT
					ret = " NOT " + value

				Case CriteriaOperation.IS
					ret = " IS " + value

				Case CriteriaOperation.BETWEEN
					ret = " BETWEEN " + value

				Case CriteriaOperation.MORE_THAN
					ret = ">" + value

				Case CriteriaOperation.LESS_THAN
					ret = "<" + value

				Case CriteriaOperation.NOT_EQUAL
					ret = "<>" + value

			End Select
			ret = ret.Replace("'''", "'")
			Return ret
		End Function

#End Region

#Region " ToString() "

		Public Overrides Function ToString() As String
			Dim sql As String = _list.Aggregate("", Function(current, item) current + (item.Logic.ToString() + " " + item.Criteria + " "))

			sql = sql.Trim

			If _
				sql.StartsWith(CriteriaLogic.OR.ToString) Or sql.StartsWith(CriteriaLogic.AND.ToString) Or
				sql.StartsWith(CriteriaLogic.NOT.ToString) Then
				sql = sql.Substring(sql.IndexOf(" ", StringComparison.Ordinal) + 1)
			End If

			Return sql
		End Function

#End Region

#Region " CriteriaLogic "

		Public Enum CriteriaLogic
			[OR]
			[AND]
			[NOT]
		End Enum

#End Region

#Region " CriteriaOperation "

		Public Enum CriteriaOperation
			[LIKE]
			[EQUAL]
			[NOT_LIKE]
			[NOT]
			[NOT_EQUAL]
			[MORE_THAN]
			[LESS_THAN]
			[IS]
			[BETWEEN]
		End Enum

#End Region
	End Class
End Namespace


