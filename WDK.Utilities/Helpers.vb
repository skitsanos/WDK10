Imports System.Web.UI
Imports System.Collections.Generic
Imports System.Net.Mail
Imports System.Web.Hosting
Imports System.Web.Configuration
Imports System.Linq
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.IO
Imports Microsoft.Win32
Imports System.Text

Public Class Helpers

#Region " FindControlRecursive "

	Public Shared Function FindControlRecursive(ByVal root As Control, ByVal id As String) As Control
		If root.ID = id Then Return root

		For Each foundControl As Control In From ctl As Control In root.Controls Select foundControl1 = FindControlRecursive(ctl, id) Where (foundControl1 IsNot Nothing)
			Return foundControl
		Next

		Return Nothing
	End Function

#End Region

#Region " AppSettings "

	Friend Shared Function AppSettings(ByVal key As String) As Object
		Return WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath).AppSettings.Settings(key).Value
	End Function

#End Region

#Region " .SendMail() "

Private Shared Function acceptAllCertifications(ByVal sender As Object, ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
Return True
End Function


	Public Shared Function SendMail(ByVal fromAddress As String, ByVal toAddress As String, ByVal ccAddress As String, ByVal msgSubject As String, ByVal msgBody As String, Optional ByVal useSSL As Boolean = True) As Boolean
		If Not isValidEmail(fromAddress) Then Return False
		If Not isValidEmail(toAddress) Then Return False

		Try
			Dim smtp As New SmtpClient
			smtp.EnableSsl = useSSL

			Dim msg As New MailMessage(fromAddress, toAddress)
			msg.ReplyToList.Add(fromAddress)

			msg.From = New MailAddress(fromAddress)

			msg.Headers.Add("X-Mailer", "Skitsanos WDK")
			If ccAddress <> "" AndAlso isValidEmail(ccAddress) Then msg.Bcc.Add(New MailAddress(ccAddress))
			msg.Subject = msgSubject
			msg.IsBodyHtml = True
			msg.Body = msgBody

			ServicePointManager.ServerCertificateValidationCallback = AddressOf acceptAllCertifications

			smtp.Send(msg)

			Return True

		Catch ex As Exception
			Throw New Exception("{WDK.Helpers.SendMail} " & ex.ToString)
		End Try
	End Function

	Public Shared Function SendMail(ByVal fromAddress As String, ByVal toAddress As String, ByVal cc As List(Of String), ByVal msgSubject As String, ByVal msgBody As String, Optional ByVal useSSL As Boolean = True) As Boolean
		If Not isValidEmail(fromAddress) Then Return False
		If Not isValidEmail(toAddress) Then Return False

		Try
			Dim smtp As New SmtpClient
			smtp.EnableSsl = useSSL

			Dim msg As New MailMessage(fromAddress, toAddress)
			msg.ReplyToList.Add(fromAddress)

			msg.From = New MailAddress(fromAddress)

			msg.Headers.Add("X-Mailer", "Skitsanos WDK")
			If cc.Count > 0 Then
				For Each addy As String In From addy1 In cc Where isValidEmail(addy1)
					msg.Bcc.Add(New MailAddress(addy))
				Next
			End If

			msg.Subject = msgSubject
			msg.IsBodyHtml = True
			msg.Body = msgBody

			ServicePointManager.ServerCertificateValidationCallback = AddressOf acceptAllCertifications

			smtp.Send(msg)

			Return True

		Catch ex As Exception
			Throw New Exception("{WDK.Helpers.SendMail} " & ex.ToString)
		End Try
	End Function

	Public Shared Function SendMail(ByVal fromAddress As String,
									ByVal toAddress As String,
									ByVal cc As List(Of String),
									ByVal msgSubject As String,
									ByVal msgBody As String,
									smtpHost As String,
									smtpPort As Integer,
									smtpUsername As String,
									smtpPassword As String,
									Optional ByVal useSSL As Boolean = True,
									Optional attachments As List(Of Attachment) = Nothing) As Boolean
		If Not isValidEmail(fromAddress) Then Return False
		If Not isValidEmail(toAddress) Then Return False

		Try
			Dim smtp As New SmtpClient
			smtp.Host = smtpHost
			smtp.Port = smtpPort
			smtp.Credentials = New NetworkCredential(smtpUsername, smtpPassword)
			smtp.EnableSsl = useSSL

			Dim msg As New MailMessage(fromAddress, toAddress)
			msg.ReplyToList.Add(fromAddress)

			msg.From = New MailAddress(fromAddress)

			msg.Headers.Add("X-Mailer", "Skitsanos WDK")
			If cc.Count > 0 Then
				For Each addy As String In From addy1 In cc Where isValidEmail(addy1)
					msg.Bcc.Add(New MailAddress(addy))
				Next
			End If

			msg.Subject = msgSubject
			msg.IsBodyHtml = True
			msg.Body = msgBody

			If (attachments IsNot Nothing) Then
				For Each att As Attachment In attachments
					msg.Attachments.Add(att)
				Next
			End If

			ServicePointManager.ServerCertificateValidationCallback = AddressOf acceptAllCertifications

			smtp.Send(msg)

			Return True

		Catch ex As Exception
			Throw New Exception("{WDK.Helpers.SendMail} " & ex.ToString)
		End Try
	End Function

	Public Shared Function SendMail(ByVal fromAddress As String,
									ByVal toAddress As String,
									ByVal cc As List(Of String),
									ByVal msgSubject As String,
									ByVal msgBody As String,
									Optional attachments As List(Of Attachment) = Nothing) As Boolean
		If Not isValidEmail(fromAddress) Then Return False
		If Not isValidEmail(toAddress) Then Return False

		Try
			Dim smtp As New SmtpClient
			smtp.Host = "127.0.0.1"
			smtp.Port = 25

			Dim msg As New MailMessage(fromAddress, toAddress)
			msg.ReplyToList.Add(fromAddress)

			msg.From = New MailAddress(fromAddress)

			msg.Headers.Add("X-Mailer", "Skitsanos WDK")
			If cc.Count > 0 Then
				For Each addy As String In From addy1 In cc Where isValidEmail(addy1)
					msg.Bcc.Add(New MailAddress(addy))
				Next
			End If

			msg.Subject = msgSubject
			msg.IsBodyHtml = True
			msg.Body = msgBody

			If (attachments IsNot Nothing) Then
				For Each att As Attachment In attachments
					msg.Attachments.Add(att)
				Next
			End If

			ServicePointManager.ServerCertificateValidationCallback = AddressOf acceptAllCertifications
			smtp.Send(msg)

			Return True

		Catch ex As Exception
			Throw New Exception("{WDK.Helpers.SendMail} " & ex.ToString)
		End Try
	End Function

#End Region

#Region " .IsLeapYear() "

	Public Shared Function IsLeapYear(ByVal year As Integer) As Boolean
		Return DateTime.IsLeapYear(year)
	End Function

#End Region

#Region " .IsValidIP() "

	Public Shared Function IsValidIp(ByVal address As String) As Boolean _
		'has to be in the dotted aaa.bbb.ccc.ddd with a..d from 0 to 255
		Dim vBytes As String() = Split(Trim(address), ".")
		If UBound(vBytes) = 3 Then
			If IsNumeric(vBytes(0)) And IsNumeric(vBytes(1)) And IsNumeric(vBytes(2)) And IsNumeric(vBytes(3)) Then
				If _
					vBytes(0) >= 0 And vBytes(0) <= 255 And vBytes(1) >= 0 And vBytes(1) <= 255 And vBytes(2) >= 0 And vBytes(2) <= 255 And
					vBytes(3) >= 0 And vBytes(3) <= 255 Then IsValidIp = True
			Else
				Throw New Exception("IP address invalid")
			End If
		End If
	End Function

#End Region

#Region " isValidEmail "

	Public Shared Function isValidEmail(ByVal address As String) As Boolean
		address = address.Trim
		Const strRegex As String = "^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"
		Dim re As New Regex(strRegex)
		If (re.IsMatch(address)) Then
			Return True
		Else
			Return False
		End If
	End Function

#End Region

#Region " .GeneratePassword() "

	Public Shared Function generatePassword(ByVal passwordLength As Integer) As String
		Dim genPassword As String = ""
		Randomize()

		Dim intNum As Integer
		Dim intUpper As Integer
		Dim intLower As Integer
		Dim intRand As Integer
		Dim strPartPass As String = ""

		For counter As Integer = 1 To passwordLength
			intNum = Int(10 * Rnd() + 48)
			intUpper = Int(26 * Rnd() + 65)
			intLower = Int(26 * Rnd() + 97)
			intRand = Int(3 * Rnd() + 1)
			Select Case intRand
				Case 1
					strPartPass = ChrW(intNum)
				Case 2
					strPartPass = ChrW(intUpper)
				Case 3
					strPartPass = ChrW(intLower)
			End Select
			genPassword = genPassword & strPartPass
		Next

		Return genPassword
	End Function

#End Region

#Region " .GUID() "

	Public Shared Function GUID() As String
		Return System.Guid.NewGuid().ToString()
	End Function

#End Region

#Region " .GetComputerName() "

	Public Shared Function GetComputerName() As String
		Return Environment.GetEnvironmentVariable("COMPUTERNAME")
	End Function

#End Region

#Region " .BrakeFrames() "

	Public Shared Function BrakeFrames() As String
		Dim tmpStr As String = ""
		tmpStr += "<script type=""text/javascript"">" & vbCrLf
		tmpStr += "if (window!=window.top)" & vbCrLf
		tmpStr += "top.location.href=location.href;"
		tmpStr += "</script>"
		Return tmpStr
	End Function

#End Region

#Region " KeepFrames "

	Public Shared Function KeepFrames(ByVal Url As String, Optional ByVal Frame As String = "top") As String
		Dim html As String = "<script type=""text/javascript"">"
		html += "if (window==window." & Frame & ")"
		html += "top.location.href=""" & Url & """;"
		html += "</script>"
		Return html
	End Function

#End Region

#Region " .IsObjectInstalled() "

	Public Shared Function isObjectInstalled(ByVal objectName As String) As Boolean
		Try

			Dim tmpObj As Object = CreateObject(objectName)
			If tmpObj Is Nothing Then
				Return False
			Else
				Return True
			End If

		Catch ex As Exception
			Return False
		End Try
	End Function

#End Region

#Region " .ExtractValue() "

	Public Shared Function extractValue(ByVal variableName As String, ByVal queryString As String, Optional ByVal splitter As String = "&") As String
		If queryString = "" Then Return ""

		Dim myVar As String() = Split(queryString, splitter)
		Dim maxVar As Integer = UBound(myVar)

		Dim x As Integer
		Dim extVar As String
		Dim res As String = ""

		For x = 0 To maxVar
			extVar = Mid(myVar(x), 1, InStr(1, myVar(x), "=") - 1)
			If variableName = extVar Then
				res = Mid(myVar(x), InStr(1, myVar(x), "=") + 1)
			End If
		Next

		Return res
	End Function

#End Region

#Region " GetODBCDriversList() "

	Public Shared Function getOdbcDriversList() As String()
		'HKEY_LOCAL_MACHINE\SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers
		Dim regKey As RegistryKey =
				Registry.LocalMachine.OpenSubKey("SOFTWARE", False).OpenSubKey("ODBC", False).OpenSubKey("ODBCINST.INI", False).
				OpenSubKey("ODBC Drivers", False)

		Return regKey.GetValueNames
	End Function

#End Region

#Region " removeAccents "

	Public Shared Function removeAccents(ByVal src As String) As String
		Dim strAccents As String() = New String() _
				{"À", "Á", "Â", "Ã", "Ä", "Ç", "È", "É", "Ê", "Ë", "Ì", "Í", "Î", "Ï", "Ñ", "Ò", "Ó", "Ô", "Õ", "Ö", "Ù", "Ú",
				 "Û", "Ü", "ß", "à", "á", "â", "ã", "ä", "ç", "è", "é", "ê", "ë", "ì", "í", "î", "ï", "ñ", "ò", "ó", "ô", "õ",
				 "ö", "ù", "ú", "û", "ü"}
		Dim strRemoveAccents As String() = New String() _
				{"&Agrave;", "&Aacute;", "&Acirc;", "&Atilde;", "&Auml;", "&Ccedil;", "&Egrave;", "&Eacute;", "&Ecirc;", "&Euml;",
				 "&Igrave;", "&Iacute;", "&Icirc;", "&Iuml;", "&Ntilde;", "&Ograve;", "&Oacute;", "&Ocirc;", "&Otilde;", "&Ouml;",
				 "&Ugrave;", "&Uacute;", "&Ucirc;", "&Uuml;", "&szlig;", "&agrave;", "&aacute;", "&acirc;", "&atilde;", "&auml;",
				 "&ccedil;", "&egrave;", "&eacute;", "&ecirc;", "&euml;", "&igrave;", "&iacute;", "&icirc;", "&iuml;", "&ntilde;",
				 "&ograve;", "&oacute;", "&ocirc;", "&otilde;", "&ouml;", "&ugrave;", "&uacute;", "&ucirc;", "&uuml;"}

		For q As Integer = 0 To UBound(strAccents)
			If InStr(src, strAccents(q)) Then
				src = src.Replace(strAccents(q), strRemoveAccents(q))
			End If
		Next

		Return src
	End Function

#End Region

#Region " createAccents "

	Public Shared Function createAccents(ByVal src As String) As String
		Dim strAccents As String() = New String() _
				{"À", "Á", "Â", "Ã", "Ä", "Ç", "È", "É", "Ê", "Ë", "Ì", "Í", "Î", "Ï", "Ñ", "Ò", "Ó", "Ô", "Õ", "Ö", "Ù", "Ú",
				 "Û", "Ü", "ß", "à", "á", "â", "ã", "ä", "ç", "è", "é", "ê", "ë", "ì", "í", "î", "ï", "ñ", "ò", "ó", "ô", "õ",
				 "ö", "ù", "ú", "û", "ü"}
		Dim strRemoveAccents As String() = New String() _
				{"&Agrave;", "&Aacute;", "&Acirc;", "&Atilde;", "&Auml;", "&Ccedil;", "&Egrave;", "&Eacute;", "&Ecirc;", "&Euml;",
				 "&Igrave;", "&Iacute;", "&Icirc;", "&Iuml;", "&Ntilde;", "&Ograve;", "&Oacute;", "&Ocirc;", "&Otilde;", "&Ouml;",
				 "&Ugrave;", "&Uacute;", "&Ucirc;", "&Uuml;", "&szlig;", "&agrave;", "&aacute;", "&acirc;", "&atilde;", "&auml;",
				 "&ccedil;", "&egrave;", "&eacute;", "&ecirc;", "&euml;", "&igrave;", "&iacute;", "&icirc;", "&iuml;", "&ntilde;",
				 "&ograve;", "&oacute;", "&ocirc;", "&otilde;", "&ouml;", "&ugrave;", "&uacute;", "&ucirc;", "&uuml;"}

		For q As Integer = 0 To UBound(strAccents)
			If InStr(src, strAccents(q)) Then
				src = src.Replace(strRemoveAccents(q), strAccents(q))
			End If
		Next

		Return src
	End Function

#End Region

#Region " getTinyUrl "

	Public Shared Function getTinyUrl(ByVal url As String) As String
		If url.Length <= 12 Then Return url

		If Not url.ToLower.StartsWith("http") And Not url.ToLower.StartsWith("ftp") Then
			url = "http://" + url
		End If

		Dim request As WebRequest = WebRequest.Create("http://tinyurl.com/api-create.php?url=" + url)
		Dim res As WebResponse = request.GetResponse

		Dim temp As String

		Using reader As New StreamReader(res.GetResponseStream)
			temp = reader.ReadToEnd
		End Using

		Return temp
	End Function

#End Region

#Region " Log "

	Public Shared Sub log(ByVal data As String)
		File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "wdk.log", data)
	End Sub

#End Region

#Region " stripHtml "

	Public Shared Function stripHtml(ByVal htmlString As String) As String
		'(<[^>]+>) 
		Return Regex.Replace(htmlString, "<(.|\n)*?>", String.Empty)
	End Function

#End Region

	Public Shared Function hexToBytes(ByVal hexString As String) As Byte()
		' allocate byte array based on half of string length
		Dim numBytes As Integer = (hexString.Length) / 2
		Dim bytes As Byte() = New Byte(numBytes - 1) {}

		' loop through the string - 2 bytes at a time converting it to decimal equivalent and store in byte array
		' x variable used to hold byte array element position
		For x As Integer = 0 To numBytes - 1
			bytes(x) = Convert.ToByte(hexString.Substring(x * 2, 2), 16)
		Next

		' return the finished byte array of decimal values
		Return bytes
	End Function

	Public Shared Function bytesToHex(ByVal data As Byte()) As String
		Dim strTemp As New StringBuilder(data.Length * 2)
		For Each b As Byte In data
			strTemp.Append(b.ToString("x2"))
		Next
		Return strTemp.ToString()
	End Function
End Class