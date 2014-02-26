Imports System.Text.RegularExpressions

Public Class W3CDateTime
	Implements IComparable

#Region " Properties "

	Private dtime As DateTime  ' The time in the local time zone
	Private ofs As TimeSpan	  ' offset from dtime to universal time

	Private Shared ReadOnly _
		MonthNames() As String = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}

	Public ReadOnly Property dateTime() As DateTime
		Get
			Return dtime
		End Get
	End Property

	Public ReadOnly Property localTime() As DateTime
		Get
			Return UtcTime.Add(localUtcOffset)
		End Get
	End Property

	Public Shared ReadOnly Property localUtcOffset() As TimeSpan
		Get
			Dim xNow As DateTime = Date.Now
			Return xNow.Subtract(xNow.ToUniversalTime())
		End Get
	End Property

	Public Shared ReadOnly Property maxValue() As W3CDateTime
		Get
			Return New W3CDateTime(Date.MaxValue, TimeSpan.Zero)
		End Get
	End Property

	Public Shared ReadOnly Property minValue() As W3CDateTime
		Get
			Return New W3CDateTime(Date.MinValue, TimeSpan.Zero)
		End Get
	End Property

	Public Shared Property Now() As W3CDateTime
		Get
			Return New W3CDateTime(Date.Now)
		End Get
		Set(value As W3CDateTime)
		End Set
	End Property

	Public ReadOnly Property offset() As TimeSpan
		Get
			Return ofs
		End Get
	End Property

	Public ReadOnly Property utcTime() As DateTime
		Get
			Return dtime.Subtract(ofs)
		End Get
	End Property

#End Region

#Region " Constructors "

	Public Sub New(ByVal dt As DateTime, ByVal off As TimeSpan)
		dtime = dt
		ofs = off
	End Sub

	Public Sub New(ByVal dt As DateTime)
		Me.New(dt, localUtcOffset)
	End Sub

#End Region

#Region " Add() "

	Public Function Add(ByVal val As TimeSpan) As W3CDateTime
		Return New W3CDateTime(dtime.Add(val), Me.ofs)
	End Function

#End Region

#Region " AddDays() "

	Public Function AddDays(ByVal val As Double) As W3CDateTime
		Return New W3CDateTime(dtime.AddDays(val), Me.ofs)
	End Function

#End Region

	Public Function AddHours(ByVal val As Double) As W3CDateTime
		Return New W3CDateTime(dtime.AddHours(val), Me.ofs)
	End Function

	Public Function AddMilliseconds(ByVal val As Double) As W3CDateTime
		Return New W3CDateTime(dtime.AddMilliseconds(val), Me.ofs)
	End Function

	Public Function AddMinutes(ByVal val As Double) As W3CDateTime
		Return New W3CDateTime(dtime.AddMinutes(val), Me.ofs)
	End Function

	Public Function AddMonths(ByVal val As Integer) As W3CDateTime
		Return New W3CDateTime(dtime.AddMonths(val), Me.ofs)
	End Function

	Public Function AddSeconds(ByVal val As Double) As W3CDateTime
		Return New W3CDateTime(dtime.AddSeconds(val), Me.ofs)
	End Function

	Public Function AddTicks(ByVal val As Long) As W3CDateTime
		Return New W3CDateTime(dtime.AddTicks(val), Me.ofs)
	End Function

	Public Function AddYears(ByVal val As Integer) As W3CDateTime
		Return New W3CDateTime(dtime.AddYears(val), Me.ofs)
	End Function

	Public Shared Function Compare(ByVal t1 As W3CDateTime, ByVal t2 As W3CDateTime) As Integer
		Return Date.Compare(t1.UtcTime, t2.UtcTime)
	End Function

	Public Overloads Overrides Function Equals(ByVal obj As Object) As Boolean
		If obj = Nothing Or Not Me.GetType() Is obj.GetType() Then
			Return False
		End If
		Return Date.Equals(Me.UtcTime, CType(obj, W3CDateTime).UtcTime)
	End Function

	Public Overloads Shared Function Equals(ByVal t1 As W3CDateTime, ByVal t2 As W3CDateTime) As Boolean
		Return Date.Equals(t1.UtcTime, t2.UtcTime)
	End Function

	Public Overrides Function GetHashCode() As Integer
		Return dtime.GetHashCode() ^ Me.ofs.GetHashCode()
	End Function

	Public Overloads Function Subtract(ByVal val As W3CDateTime) As TimeSpan
		Return UtcTime.Subtract(val.UtcTime)
	End Function

	Public Overloads Function Subtract(ByVal val As TimeSpan) As W3CDateTime
		Return New W3CDateTime(dtime.Subtract(val), Me.ofs)
	End Function

	Public Overloads Function ToLocalTime(ByVal offset As TimeSpan) As W3CDateTime
		Return New W3CDateTime(UtcTime.Subtract(offset), offset)
	End Function

	Public Overloads Function ToLocalTime() As W3CDateTime
		Return ToLocalTime(localUtcOffset)
	End Function

	Public Function ToUniversalTime() As W3CDateTime
		Return New W3CDateTime(UtcTime, TimeSpan.Zero)
	End Function

#Region " Parse() "

	Public Shared Function Parse(ByVal s As String) As W3CDateTime
		Try
			Dim d As DateTime = Date.Parse(s)

			Dim UtcDate As New DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Millisecond)
			'Return New W3CDateTime(UtcDate.Subtract(ofs), ofs)
			Return New W3CDateTime(UtcDate)

		Catch ex As Exception
			Throw New FormatException("String is not a valid date time stamp.", ex)
		End Try
	End Function

#End Region

#Region " ParseField() "

	Private Shared Function ParseField(ByVal g As Group, ByVal defaultVal As Integer) As Integer
		If g.Success Then
			Return Integer.Parse(g.Value)
		Else
			Return defaultVal
		End If
	End Function

#End Region


	Private Shared Function ParseRfc822Month(ByVal monthName As String) As Integer
		Dim i As Integer
		For i = 0 To 11
			If monthName.CompareTo(MonthNames(i)) = 0 Then
				Return i + 1
			End If
		Next
		Throw New ApplicationException("Invalid month name")
	End Function

#Region " ParseRfc822Offset() "

	Private Shared Function ParseRfc822Offset(ByVal s As String) As TimeSpan
		If s.Length = 0 Then Return TimeSpan.Zero

		Dim hours As Integer = 0
		Select Case s
			Case "UT", "GMT"

				hours = 0
			Case "EDT"
				hours = -4
			Case "EST", "CDT"
				hours = -5
			Case "CST", "MDT"
				hours = -6
			Case "MST", "PDT"

				hours = -7
			Case "PST"
				hours = -8
			Case Else
				If s.Chars(0) = "+" Then
					Dim sfmt As String = s.Substring(1, 2) + ":" + s.Substring(3, 2)
					Return TimeSpan.Parse(sfmt)
				Else
					Return TimeSpan.Parse(s.Insert(s.Length - 2, ":"))
				End If
		End Select
		Return TimeSpan.FromHours(hours)
	End Function

#End Region

#Region " ParseW3COffset() "

	Private Shared Function ParseW3COffset(ByVal s As String) As TimeSpan
		If s.Length = 0 Or s.CompareTo("Z") = 0 Then
			Return TimeSpan.Zero
		Else
			If s.Chars(0) = "+" Then
				Return TimeSpan.Parse(s.Substring(1))
			Else
				Return TimeSpan.Parse(s)
			End If
		End If
	End Function

#End Region

#Region " ToString() "

	Public Overloads Function ToString(ByVal format As String) As String
		Select Case format
			Case "R"
				Return dtime.Add(ofs).ToString("ddd, dd MMM yyyy HH\:mm\:ss ") & FormatOffset(ofs, "")

			Case "W"
				Return dtime.Add(ofs).ToString("yyyy\-MM\-ddTHH\:mm\:ss") & FormatOffset(ofs, ":")

			Case Else
				Throw New ArgumentException("Unrecognized date format requested.")
		End Select
	End Function

#End Region

#Region " FormatOffset() "

	Private Shared Function FormatOffset(ByVal ofs As TimeSpan, ByVal separator As String) As String
		Dim s As String = String.Empty
		If ofs.CompareTo(TimeSpan.Zero) >= 0 Then s = s & "+"
		Return s & ofs.Hours.ToString("00") & separator & ofs.Minutes.ToString("00")
	End Function

#End Region

#Region " CompareTo () "

	Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo
		If obj = Nothing Then
			Return 1
		End If

		If obj.GetType() Is Me.GetType() Then
			Return Compare(Me, CType(obj, W3CDateTime))
		End If
		Throw New ArgumentException("Don't know how to compare")
	End Function

#End Region
End Class