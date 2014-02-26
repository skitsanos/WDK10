Imports System.Web

Public Class SystemInformation

#Region " Properties "

	Public ReadOnly Property ComputerName() As String
		Get
			Return Environment.MachineName
		End Get
	End Property

	Public ReadOnly Property Username() As String
		Get
			Return Environment.GetEnvironmentVariable("USERNAME")
		End Get
	End Property

	Public ReadOnly Property Cpu() As String
		Get
			Return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")
		End Get
	End Property

	Public ReadOnly Property CpuArchitecture() As String
		Get
			Return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")
		End Get
	End Property

	Public ReadOnly Property NumberOfProcessors() As String
		Get
			Return Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS")
		End Get
	End Property

	Public ReadOnly Property ServicePack() As String
		Get
			Return Environment.OSVersion.ServicePack
		End Get
	End Property

	Public ReadOnly Property OSVersion() As String
		Get
			Return My.Computer.Info.OSVersion
		End Get
	End Property

	Public ReadOnly Property Os() As String
		Get
			Return My.Computer.Info.OSFullName
		End Get
	End Property

	Public ReadOnly Property Platform() As String
		Get
			Return My.Computer.Info.OSPlatform
		End Get
	End Property

	Public ReadOnly Property TotalPhysicalMemory() As Long
		Get
			Return My.Computer.Info.TotalPhysicalMemory
		End Get
	End Property

	Public ReadOnly Property WindowsDirectory() As String
		Get
			Return Environment.GetEnvironmentVariable("windir")
		End Get
	End Property

	Public ReadOnly Property SystemDirectory() As String
		Get
			Return Environment.SystemDirectory
		End Get
	End Property

	Public ReadOnly Property RemoteAddress() As String
		Get
			Return HttpContext.Current.Request.ServerVariables("REMOTE_ADDR")
		End Get
	End Property

#End Region
End Class
