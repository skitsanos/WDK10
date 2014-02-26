Imports System.Reflection
Imports System.Messaging

'Enumeration of queue message formats
Public Enum QueueMessageFormat
	ActiveX
	Binary
	XmlString
	[Default]
End Enum

Public Class MSMQ
	Private Const strErr As String = "ERROR"
	Private Shared _strAppName As String = MethodBase.GetCurrentMethod().ReflectedType.Name
	Private Const strNoMessage As String = "NOMESSAGE"


	''' <summary>
	'''     Contains string that returned when there are no messages in the queue
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Shared ReadOnly Property NoMessageAlert() As String
		Get
			Return (strNoMessage)
		End Get
	End Property

	
	''' <summary>
	'''     Sends data to the queue.
	''' </summary>
	''' <param name="strQueueName"></param>
	''' <param name="strMessage"></param>
	''' <param name="strMessageLabel"></param>
	''' <param name="formatter"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Shared Function SendPrivate(ByVal strQueueName As String, ByVal strMessage As String,
	                                   ByVal strMessageLabel As String, ByVal formatter As QueueMessageFormat) As Boolean
		Try
			Dim moneeQueue As New MessageQueue(".\Private$\" + strQueueName)
			Dim messageToSend As New Message(strMessage)
			messageToSend.Label = strMessageLabel
			Select Case formatter
				Case QueueMessageFormat.ActiveX
					messageToSend.Formatter = New ActiveXMessageFormatter()
					Exit Select
				Case QueueMessageFormat.Binary
					messageToSend.Formatter = New BinaryMessageFormatter()
					Exit Select
				Case QueueMessageFormat.XmlString
					messageToSend.Formatter = New XmlMessageFormatter(New Type() {GetType(String)})
					Exit Select
				Case QueueMessageFormat.[Default]
					messageToSend.Formatter = New XmlMessageFormatter(New Type() {GetType(String)})
					Exit Select
				Case Else
					messageToSend.Formatter = New XmlMessageFormatter(New Type() {GetType(String)})
					Exit Select
			End Select
			moneeQueue.Send(messageToSend, MessageQueueTransactionType.Automatic)
			moneeQueue.Close()
			Return True
		Catch e As Exception
			Return False
		End Try
	End Function

	
	''' <summary>
	'''     Gets data from the queue
	''' </summary>
	''' <param name="strQueueName"></param>
	''' <param name="formatter"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Shared Function GetMessage(ByVal strQueueName As String, ByVal formatter As QueueMessageFormat) As String
		Try
			Dim moneeQueue As New MessageQueue(".\Private$\" + strQueueName)
			Select Case formatter
				Case QueueMessageFormat.ActiveX
					moneeQueue.Formatter = New ActiveXMessageFormatter()
					Exit Select
				Case QueueMessageFormat.Binary
					moneeQueue.Formatter = New BinaryMessageFormatter()
					Exit Select
				Case QueueMessageFormat.XmlString
					moneeQueue.Formatter = New XmlMessageFormatter(New Type() {GetType(String)})
					Exit Select
				Case QueueMessageFormat.[Default]
					moneeQueue.Formatter = New ActiveXMessageFormatter()
					Exit Select
				Case Else
					moneeQueue.Formatter = New XmlMessageFormatter(New Type() {GetType(String)})
					Exit Select
			End Select
			Dim messageToReturn As Message = moneeQueue.Receive(New TimeSpan(0, 0, 1), MessageQueueTransactionType.Automatic)
			Return messageToReturn.Body.ToString()
		Catch ex As MessageQueueException
			If ex.MessageQueueErrorCode = MessageQueueErrorCode.IOTimeout Then
				Return strNoMessage
			Else
				Return strERR
			End If
		Catch e As Exception
			Return strERR
		End Try
	End Function

	
	''' <summary>
	'''     Creates private queue
	''' </summary>
	''' <param name="strQueueName"></param>
	''' <param name="isTransactional"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Shared Function CreateQueue(ByVal strQueueName As String, ByVal isTransactional As Boolean) As Boolean
		Try
			strQueueName = ".\Private$\" + strQueueName
			If Not MessageQueue.Exists(strQueueName) Then
				MessageQueue.Create(strQueueName, isTransactional)
			End If
			Return True
		Catch e As Exception
			Return False
		End Try
	End Function
End Class
