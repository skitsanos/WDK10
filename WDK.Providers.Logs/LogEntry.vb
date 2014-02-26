Imports System.Web

Public Class LogEntryType

	Public createdOn As DateTime = Now
	Public content As String
	Public isError As Boolean
	Public userAgent As String = HttpContext.Current.Request.UserAgent
	Public host As String = HttpContext.Current.Request.Url.Host
	Public type As String = "LogEntryType"
End Class