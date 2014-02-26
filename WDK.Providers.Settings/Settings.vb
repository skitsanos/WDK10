Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Web

Public Class [Settings]

	'function(doc) {if(doc.type=='ApplicationSettingType')\n  emit(doc.key, doc.value);}

#Region " Set "
	Public Shared Sub [Set](ByVal propertyName As String, ByVal propertyValue As String)
		If [Get](propertyName) Is Nothing Then
			Add(propertyName, propertyValue)
		Else
			Update(propertyName, propertyValue)
		End If
	End Sub
#End Region

#Region " .Add() "
	Private Shared Function Add(ByVal propertyName As String, ByVal propertyValue As String) As Boolean
		Dim ret As Boolean = False
		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
		Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		Try
			Dim setting As New ApplicationSettingType
			setting.key = propertyName
			setting.value = propertyValue

			If db.createDocument(settings.db, setting) IsNot Nothing Then ret = True

		Catch ex As Exception
			Log(ex.ToString, True)
		End Try

		Return ret
	End Function
#End Region

#Region " Update() "
	Private Shared Function Update(ByVal propertyName As String, ByVal propertyValue As String) As Boolean
		Dim ret As Boolean = False

		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
		Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		Try

			Dim result As String = db.getDesignViewAsJson(settings.db, "ApplicationSettings", "showAll?key=" + HttpUtility.UrlEncode(Chr(34) + propertyName + Chr(34)))
			Dim jo As JObject = DirectCast(JsonConvert.DeserializeObject(result), JObject)
			Dim rows As JArray = jo.Item("rows")
			If rows.Count > 0 Then
				Dim docId As String = rows(0).Item("id")

				Dim setting As New ApplicationSettingType
				setting.key = propertyName
				setting.value = PropertyValue

				db.updateDocument(settings.db, docId, setting)

			End If

			ret = True

		Catch ex As Exception
			Log(ex.ToString, True)
		End Try

		Return ret
	End Function
#End Region

#Region " .Delete() "
	Public Shared Function Delete(ByVal propertyName As String) As Boolean
		Dim ret As Boolean = False
		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
		Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		Return db.deleteDocument(settings.db, PropertyName)
	End Function
#End Region

#Region " .Get() "
	Public Shared Function [Get](ByVal propertyName As String) As String
		Dim ret As String = Nothing

		Dim settings As CouchDbConnectionSettings = getConnectionSettings()
		Dim db As New API.CouchDb.Database(settings.host, settings.port, settings.username, settings.password)

		'Dim err As String

		Try
			Dim result As String = db.getDesignViewAsJson(settings.db, "ApplicationSettings", "showAll?key=" + HttpUtility.UrlEncode(Chr(34) + PropertyName + Chr(34)))
			'err = result
			Dim jo As JObject = DirectCast(JsonConvert.DeserializeObject(result), JObject)
			Dim rows As JArray = jo.Item("rows")
			If rows.Count > 0 Then ret = rows(0).Item("value")

		Catch ex As Exception
			Log("Settings provider error: " + getApplicationName() + " requested settings value for key {" + PropertyName + "}, but there is no settings registered with such key", True)
		End Try

		Return ret
	End Function
#End Region

End Class

Class ApplicationSettingType
	Public key As String
	Public value As String
	Public type As String = "ApplicationSettingType"
End Class
