Imports System.Web

Public Class ServerVariables

#Region " Enumerations "

	Enum Variables
		QUERY_STRING
		PATH_INFO
		AUTH_TYPE

		CONTENT_LENGTH
		CONTENT_TYPE

		GATEWAY_INTERFACE
		PATH_TRANSLATED

		HTTP_COOKIE
		HTTP_ACCEPT
		HTTP_REFERER
		HTTP_USER_AGENT

		REMOTE_ADDR
		REMOTE_HOST
		REMOTE_IDENT
		REMOTE_USER

		REQUEST_METHOD

		SCRIPT_NAME
		SCRIPT_ENGINE_VERSION

		SERVER_SOFTWARE
		SERVER_NAME
		SERVER_PORT
		SERVER_PROTOCOL
		SERVER_OS
	End Enum

#End Region

#Region " Properties "

	Private _Request As HttpRequest

#End Region

#Region " New() "

	Sub New(ByVal Request As HttpRequest)
		_Request = Request
	End Sub

#End Region

#Region " Get() "

	Public Function [Get](ByVal ServerVariable As Variables) As String
		Select Case ServerVariable
			Case Else
				Return _Request.ServerVariables(ServerVariable.ToString)
		End Select
	End Function

#End Region
End Class
