Imports System
Imports System.Runtime.InteropServices
Imports System.Drawing

Namespace Utilities
    Public Class Win32
#Region " Win32 API Imports "
        <DllImport("User32.dll")> Public Shared Function GetDC(ByVal hWnd As IntPtr) As IntPtr

        End Function

        <DllImport("User32.dll")> Public Shared Function ReleaseDC(ByVal hWnd As IntPtr, ByVal hDC As IntPtr) As IntPtr

        End Function

        <DllImport("Gdi32.dll")> Public Shared Function BitBlt(ByVal hdcDest As IntPtr, ByVal nXDest As Integer, ByVal nYDest As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hdcSrc As IntPtr, ByVal nXSrc As Integer, ByVal nYSrc As Integer, ByVal dwRop As Integer) As Integer

        End Function

        <DllImport("Gdi32.dll")> Public Shared Function StretchBlt(ByVal hdcDest As IntPtr, ByVal nXOriginDest As Integer, ByVal nYOriginDest As Integer, ByVal nWidthDest As Integer, ByVal nHeightDest As Integer, ByVal hdcSrc As IntPtr, ByVal nXOriginSrc As Integer, ByVal nYOriginSrc As Integer, ByVal nWidthSrc As Integer, ByVal nHeightSrc As Integer, ByVal dwRop As Integer) As Integer

        End Function

        <DllImport("Gdi32.dll")> Public Shared Function SetROP2(ByVal hDC As IntPtr, ByVal fnDrawMode As Integer) As Integer

        End Function

        <DllImport("Gdi32.dll")> Shared Function MoveToEx(ByVal hDC As IntPtr, ByVal x As Integer, ByVal y As Integer, ByVal lpPoint As IntPtr) As Integer

        End Function

        <DllImport("Gdi32.dll")> Public Shared Function LineTo(ByVal hDC As IntPtr, ByVal nXEnd As Integer, ByVal nYEnd As Integer) As Integer

        End Function

        <DllImport("Gdi32.dll")> Shared Function GetPixel(ByVal hdc As IntPtr, ByVal nXPos As Integer, ByVal nYPos As Integer) As Integer

        End Function

        <DllImport("Gdi32.dll")> Public Shared Function SelectObject(ByVal hDC As IntPtr, ByVal hGdiObj As IntPtr) As IntPtr

        End Function

        <DllImport("Gdi32.dll")> Public Shared Function CreateCompatibleDC(ByVal hdc As IntPtr) As IntPtr

        End Function

        <DllImport("Gdi32.dll")> Public Shared Function CreateCompatibleBitmap(ByVal hDC As IntPtr, ByVal nWidth As Integer, ByVal nHeight As Integer) As IntPtr

        End Function

        <DllImport("Gdi32.dll")> Public Shared Function DeleteObject(ByVal hObject As IntPtr) As Integer

        End Function

#End Region

        Const SRCCOPY As Integer = &HCC0020
        Const R2_NOT As Integer = 6
        Const R2_NOP As Integer = 11

        Public Shared Function MoveTo(ByVal hDC As IntPtr, ByVal x As Integer, ByVal y As Integer) As Integer
            Return MoveToEx(hDC, x, y, IntPtr.Zero)
        End Function 'MoveTo

        Public Shared Function GetPixelColor(ByVal hDC As IntPtr, ByVal x As Integer, ByVal y As Integer) As Color
            Dim colorRef As Long = GetPixel(hDC, x, y)
            Return Color.FromArgb(System.Convert.ToByte(colorRef), System.Convert.ToByte(Machine.Shift.Right(colorRef, 8)), System.Convert.ToByte(Machine.Shift.Right(colorRef, 16)))
        End Function 'GetPixelColor
    End Class 'Win32

End Namespace
