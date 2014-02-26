Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Collections
Imports System.Xml.Serialization
Imports System.IO
Imports System.Xml


Public Class Serializer
    ''' <summary>
    ''' Serializing object into xml
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="Namespace"></param>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    Public Shared Function ObjectToXmlString(ByVal obj As Object, ByVal Name As String, ByVal [Namespace] As String, ByVal enc As String) As String
        Dim ms As MemoryStream = Nothing
        Dim xmlWr As XmlWriter = Nothing
        Dim ret As String = ""
        Try
            ms = New MemoryStream()
			Dim xmlSrz As XmlSerializer = SerializerCache.GetSerializer(obj.[GetType]())

            Dim xmlWrSt As New XmlWriterSettings()
            xmlWrSt.OmitXmlDeclaration = True
			xmlWrSt.Encoding = Encoding.GetEncoding(enc)

            xmlWr = XmlWriter.Create(ms, xmlWrSt)

            xmlSrz.Serialize(xmlWr, obj, New XmlSerializerNamespaces(New XmlQualifiedName() {New XmlQualifiedName(Name, [Namespace])}))
            
            ret = Encoding.GetEncoding(enc).GetString(ms.ToArray())

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Log(ex.ToString(), True)
        Finally
            If ms IsNot Nothing Then
                ms.Flush()
                ms.Close()
            End If
        End Try

        Return ret
    End Function
    ''' <summary>
    ''' Serializing object into xml
    ''' </summary>
    ''' <param name="obj"></param>    
    ''' <returns></returns>
    Public Shared Function ObjectToXmlString(ByVal obj As Object) As String
        Return ObjectToXmlString(obj, "", "", "UTF-8")
    End Function
   
    ''' <summary>
    ''' Deseriliazing xml into object
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="XmlStirng"></param>
    ''' <returns></returns>
    Public Shared Function XmlStringToObject(Of T)(ByVal XmlStirng As String) As T
        Dim ret As T = Nothing
        Try
            If XmlStirng Is Nothing Then Return (Nothing)

            If XmlStirng = String.Empty Then Return DirectCast(Activator.CreateInstance(GetType(T)), T)

            Dim ser As New XmlSerializer(GetType(T))
            Dim tr As IO.TextReader = New IO.StringReader(XmlStirng)

            ret = ser.Deserialize(tr)

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Log(ex.ToString(), True)
        End Try

        Return ret
    End Function

#Region " GetXml "
    Public Shared Function GetXml(ByVal obj As Object) As String
        Dim ret As String = ""
        Try
            Dim ns As New XmlSerializerNamespaces
            ns.Add("", "")

            Dim ms As New IO.MemoryStream

            Dim ser As New XmlSerializer(obj.GetType)
            Dim xtw As New XmlTextWriter(ms, Text.Encoding.UTF8)
            ser.Serialize(xtw, obj, ns)

            ms = xtw.BaseStream

            ret = Text.Encoding.UTF8.GetString(ms.ToArray)

        Catch ex As Exception
            Log("{Xml Serializer Error within Pages Manager} " & ex.ToString, True)
        End Try

        Return ret
    End Function


    Public Shared Function SetXml(ByVal Type As System.Type, ByVal XmlData As String) As Object
        Dim obj As Object = Nothing

        Try
            Dim ser As New XmlSerializer(Type)
            Dim tr As IO.TextReader = New IO.StringReader(XmlData)
            obj = ser.Deserialize(tr)

        Catch ex As Exception
            Log("{Xml Deserializer Error within Pages Manager} " & ex.ToString)
        End Try

        Return obj
    End Function
#End Region

End Class