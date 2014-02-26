Imports System.Net
Imports System.IO
Imports System.Text

Public Class Http

#Region " Properties "

#End Region

#Region " FetchURL() "

	Public Shared Function FetchUrl(ByVal url As String, Optional ByVal credentials As ICredentials = Nothing) As String
		If url = "" Then
			Return ""
		Else
			Try
				Dim request As WebRequest = WebRequest.Create(url)
				If credentials IsNot Nothing Then request.Credentials = credentials
				request.Headers.Add("pragma", "no-cache")
				request.Headers.Add("cache-control", "private")

				Dim res As WebResponse = request.GetResponse

				Dim temp As String
				Using reader As New StreamReader(res.GetResponseStream)
					temp = reader.ReadToEnd
				End Using

				Return temp
			Catch e As Exception
				Return e.Message
			End Try
		End If
	End Function

#End Region

#Region " FetchURL() "

	Public Shared Function FetchData(ByVal URL As String) As Byte()
		Try
			Dim webClient As WebClient = New WebClient
			webClient.Headers.Add("pragma", "no-cache")
			webClient.Headers.Add("cache-control", "private")

			Dim b As Byte() = webClient.DownloadData(URL)
			Return b

		Catch e As Exception
			Return Nothing
		End Try
	End Function

#End Region

#Region " FetchPage() "

	Public Shared Function FetchPage(ByVal pageLocation As String) As String
		If PageLocation = "" Then Return "[WDK.HTTP].FetchPage Error: PageLocation is empty."

		If File.Exists(PageLocation) Then
			Dim txtStream As Stream = File.Open(PageLocation, FileMode.Open)
			Dim bs As Byte() = New Byte(CInt(txtStream.Length) + 1) {}
			txtStream.Read(bs, 0, txtStream.Length)
			Dim strResponse As String = Encoding.UTF8.GetString(bs)
			txtStream.Close()
			Return strResponse
		Else
			Return "IO Error. Specified PageLocation not found"
		End If
	End Function

#End Region

#Region " HttpPing() "

	Public Shared Function HttpPing(ByVal URL As String) As Boolean
		Try
			Dim webClient As WebClient = New WebClient
			webClient.OpenRead(URL)
			webClient.Dispose()
			Return True
		Catch e As Exception
			Return False
		End Try
	End Function

#End Region
End Class
