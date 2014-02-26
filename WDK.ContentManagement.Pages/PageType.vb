Public Class PageType
    Public type As String = "PageType"
	Public _id As String

	Public contentType As PageContentType = PageContentType.HTML

	Public filename As String = "untitled"
	Public title As String = "untitled page"
	Public content As String = ""


	Public masterPage As String = "~/default.master"

	Public createdOn As String = Now
	Public updatedOn As String = Now

	Public Metas As New List(Of KeyValuePair(Of String, String))

	Public allowComments As Boolean = False
End Class

Public Class PageViewType
	Inherits PageType
	Public _rev As String
End Class
