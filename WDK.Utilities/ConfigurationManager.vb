Imports System.Xml
Imports System.IO

Public Class ConfigurationManager

#Region " .Properties "

	Private xmlDoc As XmlDocument

#End Region

#Region " Constructors "

	Public Sub New()
		xmlDoc = New XmlDocument

		Dim xmlProc As XmlProcessingInstruction = xmlDoc.CreateProcessingInstruction("xml",
		                                                                             "version=""1.0"" encoding=""utf-8""")
		xmlDoc.AppendChild(xmlProc)

		Dim xmlRoot As XmlElement = xmlDoc.CreateElement("configuration")
		xmlDoc.AppendChild(xmlRoot)
	End Sub

#End Region

#Region " Load() "

	Public Sub Load(ByVal File As String)
		xmlDoc.Load(File)
	End Sub

#End Region

#Region " LoadXml() "

	Public Sub LoadXml(ByVal Data As String)
		xmlDoc.LoadXml(Data)
	End Sub

#End Region

#Region " Save() "

	Public Sub Save(ByVal File As String)
		xmlDoc.Save(File)
	End Sub

	Public Sub Save(ByVal Stream As Stream)
		xmlDoc.Save(Stream)
	End Sub

#End Region

#Region " .Write() "

	Public Function Write(ByVal Section As String, ByVal PropertyName As String, ByVal PropertyValue As String) As Boolean
		Dim SectionNode As XmlElement = xmlDoc.SelectSingleNode("//" & Section)
		Dim mainNode As XmlElement = XMLDoc.DocumentElement

		If (SectionNode Is Nothing) Then
			Dim newNode As XmlElement = XMLDoc.CreateElement(Section)
			mainNode.AppendChild(newNode)
			newNode.SetAttribute(PropertyName, PropertyValue)
		Else
			SectionNode.SetAttribute(PropertyName, PropertyValue)
		End If

		Return True
	End Function

#End Region

#Region " .Read() "

	Public Function Read(ByVal Section As String, ByVal PropertyName As String) As String
		If xmlDoc.OuterXml = "" Then
			Throw New Exception("Configuration file is empty")
		End If

		Dim SettingNode As XmlElement = xmlDoc.SelectSingleNode("//" & Section)

		If (SettingNode Is Nothing) Then
			'Throw New Exception("Section [" & Section & "] is not exists")
			Return Nothing
		Else
			Dim tmpNode As XmlElement = xmlDoc.SelectSingleNode("//" & Section & "[@" & PropertyName & "!='']")
			If Not (tmpNode Is Nothing) Then
				Return SettingNode.GetAttribute(PropertyName)
			Else
				Return Nothing
			End If
		End If
	End Function

#End Region

#Region " .DeleteProperty()"

	Public Function DeleteProperty(ByVal Section As String, ByVal PropertyName As String) As Boolean
		If xmlDoc.OuterXml = "" Then
			Throw New Exception("The file does not contains any data")
		End If

		Dim SettingNode As XmlElement = xmlDoc.SelectSingleNode("//" & Section)

		If (SettingNode Is Nothing) Then
			Throw New Exception("Section [" & Section & "] not exists")
		Else
			SettingNode.Attributes.RemoveNamedItem(PropertyName)
			Return True
		End If
	End Function

#End Region

#Region " .DeleteSection()"

	Public Function DeleteSection(ByVal Section As String) As Boolean
		If xmlDoc.OuterXml = "" Then
			Throw New Exception("The file does not contains any data")
		End If

		Dim SettingNode As XmlNodeList = xmlDoc.SelectNodes("//" & Section)

		If (SettingNode Is Nothing) Then
			Throw New Exception("Error while trying to delete section " & Section & ". Section is not exists")
		Else
			Dim Node As XmlElement
			For Each Node In SettingNode
				Node.ParentNode.RemoveChild(Node)
			Next

			Return True
		End If
	End Function

#End Region

#Region " OuterXml() "

	Public Function OuterXml() As String
		Return xmlDoc.OuterXml
	End Function

#End Region
End Class