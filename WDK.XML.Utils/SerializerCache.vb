Imports System.Xml.Serialization

Friend Class SerializerCache
    Private Shared hash As New Hashtable()
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    Friend Shared Function GetSerializer(ByVal type As Type) As XmlSerializer
        Dim res As XmlSerializer = Nothing
        SyncLock hash
            res = TryCast(hash(type.FullName), XmlSerializer)
            If res Is Nothing Then
                res = New XmlSerializer(type)
                hash(type.FullName) = res
            End If
        End SyncLock
        Return res
    End Function
End Class
