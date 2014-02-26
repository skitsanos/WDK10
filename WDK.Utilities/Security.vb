Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class Security

#Region " EncryptString() "

	Public Shared Function encryptString(ByVal textToBeEncrypted As String, ByVal encryptionKey As String) As Byte()

		Dim bytValue() As Byte
		Dim bytKey() As Byte
		Dim bytEncoded() As Byte = Nothing
		Dim bytIV() As Byte = {121, 241, 10, 1, 132, 74, 11, 39, 255, 91, 45, 78, 14, 211, 22, 62}
		Dim intLength As Integer
		Dim intRemaining As Integer
		Dim objMemoryStream As New MemoryStream
		Dim objCryptoStream As CryptoStream
		Dim objRijndaelManaged As RijndaelManaged


		'   **********************************************************************
		'   ******  Strip any null character from string to be encrypted    ******
		'   **********************************************************************

		textToBeEncrypted = StripNullCharacters(textToBeEncrypted)

		'   **********************************************************************
		'   ******  Value must be within ASCII range (i.e., no DBCS chars)  ******
		'   **********************************************************************

		bytValue = Encoding.ASCII.GetBytes(textToBeEncrypted.ToCharArray)

		intLength = Len(encryptionKey)

		'   ********************************************************************
		'   ******   Encryption Key must be 256 bits long (32 bytes)      ******
		'   ******   If it is longer than 32 bytes it will be truncated.  ******
		'   ******   If it is shorter than 32 bytes it will be padded     ******
		'   ******   with upper-case Xs.                                  ****** 
		'   ********************************************************************

		If intLength >= 32 Then
			encryptionKey = Left(encryptionKey, 32)
		Else
			intLength = Len(encryptionKey)
			intRemaining = 32 - intLength
			encryptionKey = encryptionKey & StrDup(intRemaining, "X")
		End If

		bytKey = Encoding.ASCII.GetBytes(encryptionKey.ToCharArray)

		objRijndaelManaged = New RijndaelManaged

		'   ***********************************************************************
		'   ******  Create the encryptor and write value to it after it is   ******
		'   ******  converted into a byte array                              ******
		'   ***********************************************************************

		Try
		objCryptoStream = New CryptoStream(objMemoryStream, objRijndaelManaged.CreateEncryptor(bytKey, bytIV), CryptoStreamMode.Write)
			objCryptoStream.Write(bytValue, 0, bytValue.Length)

			objCryptoStream.FlushFinalBlock()

			bytEncoded = objMemoryStream.ToArray
			objMemoryStream.Close()
		Catch


		End Try

		'   ***********************************************************************
		'   ******   Return encryptes value (converted from  byte Array to   ******
		'   ******   a base64 string).  Base64 is MIME encoding)             ******
		'   ***********************************************************************

		Return bytEncoded
	End Function

#End Region

#Region " DecryptString() "

	Public Shared Function decryptString(ByVal bytDataToBeDecrypted As Byte(), ByVal decryptionKey As String) As String
		Dim bytTemp() As Byte
		Dim bytIV() As Byte = {121, 241, 10, 1, 132, 74, 11, 39, 255, 91, 45, 78, 14, 211, 22, 62}
		Dim objRijndaelManaged As New RijndaelManaged
		Dim objMemoryStream As MemoryStream
		Dim objCryptoStream As CryptoStream
		Dim bytDecryptionKey() As Byte

		Dim intLength As Integer
		Dim intRemaining As Integer
		'Dim intCtr As Integer
		Dim strReturnString As String = String.Empty

		'   ********************************************************************
		'   ******   Encryption Key must be 256 bits long (32 bytes)      ******
		'   ******   If it is longer than 32 bytes it will be truncated.  ******
		'   ******   If it is shorter than 32 bytes it will be padded     ******
		'   ******   with upper-case Xs.                                  ****** 
		'   ********************************************************************

		intLength = Len(decryptionKey)

		If intLength >= 32 Then
			decryptionKey = Left(decryptionKey, 32)
		Else
			intLength = Len(decryptionKey)
			intRemaining = 32 - intLength
			decryptionKey = decryptionKey & StrDup(intRemaining, "X")
		End If

		bytDecryptionKey = Encoding.ASCII.GetBytes(decryptionKey.ToCharArray)

		ReDim bytTemp(bytDataToBeDecrypted.Length)

		objMemoryStream = New MemoryStream(bytDataToBeDecrypted)

		'   ***********************************************************************
		'   ******  Create the decryptor and write value to it after it is   ******
		'   ******  converted into a byte array                              ******
		'   ***********************************************************************

		Try

			objCryptoStream = New CryptoStream(objMemoryStream, objRijndaelManaged.CreateDecryptor(bytDecryptionKey, bytIV), CryptoStreamMode.Read)

			objCryptoStream.Read(bytTemp, 0, bytTemp.Length)

			objCryptoStream.FlushFinalBlock()
			objMemoryStream.Close()

		Catch

		End Try

		'   *****************************************
		'   ******   Return decypted value     ******
		'   *****************************************

		Return StripNullCharacters(Encoding.ASCII.GetString(bytTemp))
	End Function

#End Region

#Region " StripNullCharacters() "

	Private Shared Function StripNullCharacters(ByVal vstrStringWithNulls As String) As String

		Dim intPosition As Integer
		Dim strStringWithOutNulls As String

		intPosition = 1
		strStringWithOutNulls = vstrStringWithNulls

		Do While intPosition > 0
			intPosition = InStr(intPosition, vstrStringWithNulls, vbNullChar)

			If intPosition > 0 Then
				strStringWithOutNulls = Left$(strStringWithOutNulls, intPosition - 1) &
										Right$(strStringWithOutNulls, Len(strStringWithOutNulls) - intPosition)
			End If

			If intPosition > strStringWithOutNulls.Length Then
				Exit Do
			End If
		Loop

		Return strStringWithOutNulls
	End Function

#End Region

#Region " getSHA1Hash "

	Public Shared Function getSHA1Hash(ByVal data As String) As Byte()
		Dim hashBytes As Byte() = Encoding.UTF8.GetBytes(data)
		Dim sha1 As New SHA1CryptoServiceProvider()
		Return sha1.ComputeHash(hashBytes)
	End Function

#End Region
End Class
