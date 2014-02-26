Imports System.Web
Imports WDK.API.CouchDb
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json

Public Class Manager

	'#Region " Add (by params)"
	'	Public Function create(ByVal filename As String, ByVal masterPage As String, ByVal title As String, ByVal keywords As String, ByVal description As String, ByVal content As String) As Boolean
	'		Dim ret As Boolean = False

	'		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
	'		Dim db As New WDK.API.CouchDb(settings.host, settings.port, settings.username, settings.password)

	'		Dim entry As New PageType
	'		entry.filename = filename
	'		entry.masterPage = masterPage
	'		entry.title = title
	'		entry.content = content

	'		entry.Metas.Add(New KeyValuePair(Of String, String)("keywords", keywords))
	'		entry.Metas.Add(New KeyValuePair(Of String, String)("description", description))

	'		Dim docinfo As WDK.API.DocumentInfo = db.createDocument(settings.db, entry)
	'		If docinfo IsNot Nothing Then ret = True

	'		Return ret
	'	End Function
	'#End Region

#Region " create "
	Public Function create(ByVal entry As PageType) As Boolean
		Dim ret As Boolean = False

		entry._id = entry.filename

		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

        Dim docinfo As DocumentInfo = db.createDocument(settings.db, entry)
		If docinfo IsNot Nothing Then ret = True

		Return ret
	End Function
#End Region

#Region " Update "
	Public Function update(ByVal id As String, ByVal entry As PageType) As Boolean
		Dim ret As Boolean = False

		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		Try

            Dim docinfo As DocumentInfo = db.updateDocument(settings.db, id, entry)
			If docinfo IsNot Nothing Then ret = True

		Catch ex As Exception
			Log(ex.ToString, True)
		End Try

		Return ret

	End Function
#End Region

#Region " remove() "
	Public Function remove(ByVal Id As String) As Boolean
		Dim ret As Boolean = False

		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		db.deleteDocument(settings.db, Id)

		Return ret
	End Function
#End Region

#Region " getDatasource() "
	Public Function getDatasource(ByVal view As String, Optional ByVal query As String = "") As List(Of PageViewType)
		Dim ret As List(Of PageViewType) = Nothing

		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		ret = db.getDesignView(Of PageViewType)(settings.db, "Pages", view + "?" + query)

		Return ret
	End Function

	Public Function getDatasourceAsJson(ByVal view As String, Optional ByVal query As String = "") As String
		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		Return db.getDesignViewAsJson(settings.db, "Pages", view + "?" + query)
	End Function
#End Region

#Region " getPageById "
	Public Shared Function getPageById(ByVal id As String) As PageType
		Dim ret As PageViewType = Nothing

		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		Dim result As String = db.getDesignViewAsJson(settings.db, "Pages", "viewAll?key=" + HttpUtility.UrlEncode(Chr(34) + id + Chr(34)))
		Dim token As JToken = JsonConvert.DeserializeObject(result)
		Dim rows As JArray = token.SelectToken("rows")
		If (rows.Count > 0) Then
			Dim doc As JToken = rows(0).SelectToken("value")
			If (doc IsNot Nothing) Then
				Dim serializer As New JsonSerializer
				ret = serializer.Deserialize(Of PageViewType)(New JTokenReader(doc))
			Else
				ret = pageNotFound(id)
			End If
		Else
			ret = pageNotFound(id)
		End If

		Return ret
	End Function
#End Region

#Region " getPageByFilename "
	Public Function getPageByFilename(ByVal filename As String) As PageViewType
		Dim ret As PageViewType = Nothing

		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		Dim result As String = db.getDesignViewAsJson(settings.db, "Pages", "viewByFilename?key=" + HttpUtility.UrlEncode(Chr(34) + filename + Chr(34)))
		Dim token As JToken = JsonConvert.DeserializeObject(result)
		Dim rows As JArray = token.SelectToken("rows")
		If (rows.Count > 0) Then
			Dim doc As JToken = rows(0).SelectToken("value")
			If (doc IsNot Nothing) Then
				Dim serializer As New JsonSerializer
				ret = serializer.Deserialize(Of PageViewType)(New JTokenReader(doc))
			Else
				ret = pageNotFound(filename)
			End If
		Else
			ret = pageNotFound(filename)
		End If

		Return ret
	End Function
#End Region


#Region " count "
	Public Function count() As Integer
		Dim ret As Integer = 0

		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
        Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		Try
			Dim result As String = db.getDesignViewAsJson(settings.db, "Pages", "count")

			Dim jo As JObject = DirectCast(JsonConvert.DeserializeObject(result), JObject)
			Dim rows As JArray = jo.Item("rows")
			If rows.Count > 0 Then
				ret = rows(0).Item("value")
			End If

		Catch ex As Exception
			Log("WDK.ContentManagement.Pages provider error on .count() method: " + ex.ToString, True)
		End Try

		Return ret
	End Function
#End Region

#Region " PageNotFound "
	Private Shared Function pageNotFound(ByVal filename As String) As PageViewType
		Dim p As New PageViewType
		p.title = "Page not found"
		p.content = "Page {" + filename + "} not found"
		Return p
	End Function
#End Region

End Class
