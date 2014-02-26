Imports System.Xml

Public Class Transformation

#Region " Xml Transform "
    Public Shared Function TransformXML(ByVal SourceElement As XmlElement, ByVal TemplatePath As String) As String
        Dim res As String = ""

        Try
            Dim sw As New System.IO.StringWriter

            Dim XSLT As New Xsl.XslCompiledTransform
            XSLT.Load(TemplatePath)

            Dim xslProc As New Xsl.XsltArgumentList

            Dim tr As New System.IO.StringReader(SourceElement.OuterXml)
            Dim xr As New XmlTextReader(tr)

            Dim ms As New System.IO.MemoryStream()

            Dim settings As New XmlWriterSettings()
            settings.OmitXmlDeclaration = True
            settings.ConformanceLevel = ConformanceLevel.Auto

            Dim writer As XmlWriter = XmlWriter.Create(ms, settings)
            XSLT.Transform(xr, xslProc, writer)
            writer.Close()

            Return Text.Encoding.UTF8.GetString(ms.ToArray())
        Catch ex As Exception
            res = ex.ToString
        End Try

        Return res
    End Function


    Public Shared Function TransformXML(ByVal Source As String, ByVal TemplatePath As String) As String
        Dim res As String = ""

        Try
            Dim sw As New System.IO.StringWriter

            Dim XSLT As New Xsl.XslCompiledTransform
            XSLT.Load(TemplatePath)

            Dim xslProc As New Xsl.XsltArgumentList

            Dim tr As New System.IO.StringReader(Source)
            Dim xr As New XmlTextReader(tr)

            Dim ms As New System.IO.MemoryStream()

            Dim settings As New XmlWriterSettings()
            settings.OmitXmlDeclaration = True
            settings.ConformanceLevel = ConformanceLevel.Auto

            Dim writer As XmlWriter = XmlWriter.Create(ms, settings)
            XSLT.Transform(xr, xslProc, writer)
            writer.Close()

            Return Text.Encoding.UTF8.GetString(ms.ToArray())
        Catch ex As Exception
            res = ex.ToString
        End Try

        Return res
    End Function
#End Region

End Class
