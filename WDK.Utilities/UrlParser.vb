Imports System.Text.RegularExpressions

Namespace Parsers
	Public Class UrlParser

#Region " Properties "

		Private _IsValidUrl As Boolean = False
		Private _URL As String = ""
		Private _Credentials As String = ""
		Private _Domain As String = ""
		Private _Port As Integer = 80
		Private _Path As String = ""
		Private _Filename As String = ""
		Private _FileExtension As String = ""
		Private _QueryString As String = ""
		Private _Protocol As String = ""

		Public ReadOnly Property IsValidUrl() As Boolean
			Get
				Return _IsValidUrl
			End Get
		End Property

		Public Property Url() As String
			Get
				Return _URL
			End Get
			Set(ByVal Value As String)
				_URL = Value
			End Set
		End Property

		Public ReadOnly Property Protocol() As String
			Get
				Return _Protocol
			End Get
		End Property

		Public ReadOnly Property Domain() As String
			Get
				Return _Domain
			End Get
		End Property

		Public ReadOnly Property Port() As Integer
			Get
				Return _Port
			End Get
		End Property

		Public ReadOnly Property Path() As String
			Get
				Return _Path
			End Get
		End Property

		Public ReadOnly Property Filename() As String
			Get
				Return _Filename
			End Get
		End Property

		Public ReadOnly Property FileExtension() As String
			Get
				Return _FileExtension
			End Get
		End Property

		Public ReadOnly Property QueryString() As String
			Get
				Return _QueryString
			End Get
		End Property

#End Region

#Region " Parse() "

		Public Sub parse()
			Try
				Const regex As String = "(?:(?<protocol>http(?:s?)|ftp)(?:\:\/\/))(?:(?<usrpwd>\w+\:\w+)(?:\@))?(?<domain>[^/\r\n\:]+)?(?<port>\:\d+)?(?<path>(?:\/.*)*\/)?(?<filename>.*?\.(?<ext>\w{2,4}))?(?<qrystr>\??(?:\w+\=[^\#]+)(?:\&?\w+\=\w+)*)*(?<bkmrk>\#.*)?"
				Const options As RegexOptions = (RegexOptions.IgnorePatternWhitespace Or RegexOptions.Multiline) Or RegexOptions.IgnoreCase
				Dim reg As New Regex(regex, options)

				Dim matchCollection As MatchCollection = reg.Matches(Url)

				If matchCollection.Count > 0 Then
					_Protocol = matchCollection(0).Result("${protocol}")
					_Credentials = matchCollection(0).Result("${usrpwd}")
					_Domain = matchCollection(0).Result("${domain}")
					If matchCollection(0).Result("${port}") <> "" Then
						_Port = Val(Mid(matchCollection(0).Result("${port}"), 2))
					End If
					_Path = matchCollection(0).Result("${path}")
					_Filename = matchCollection(0).Result("${filename}")
					_FileExtension = matchCollection(0).Result("${ext}")
					_QueryString = matchCollection(0).Result("${qrystr}")

					_IsValidUrl = True
				Else
					_IsValidUrl = False
				End If

			Catch ex As Exception
				Throw New Exception(ex.Message)
			End Try
		End Sub

#End Region

#Region " .New() "

		Sub New()
		End Sub

		Sub New(ByVal Location As String)
			If Location = "" Then
				Exit Sub
			Else
				_URL = Location
			End If

			parse()
		End Sub

#End Region

	End Class
End Namespace

