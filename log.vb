Imports System.IO

Module log
#Region "log io"

	'============================================================
	'   過去履歴
	Public Structure ST_LOG
		Dim fromFolder As String
		Dim toFolder As String
		Dim skipDel As Integer
	End Structure

	Public g_logs As New ArrayList

	'============================================================
	'   read
	'============================================================
	Public Function ini_read() As Boolean

		Dim sztemp As String

		g_logs.Clear()

		Try

			Dim StreamReader = New System.IO.StreamReader(My.Application.Info.DirectoryPath + "\log.ini", System.Text.Encoding.GetEncoding(932))

			Do While StreamReader.Peek() >= 0

				'行読み込み
				sztemp = StreamReader.ReadLine()
				'カンマ区切り
				Dim tokens As String() = sztemp.Split(","c)

				Dim logEntry As ST_LOG
				logEntry.fromFolder = tokens(0)
				logEntry.toFolder = tokens(1)
				logEntry.skipDel = Val(tokens(2))
				g_logs.Add(logEntry)
			Loop
			StreamReader.Close()


		Catch ex As Exception

		End Try


	End Function

	'============================================================
	'   Write to a file. ファイルに書き込む 
	'============================================================
	Public Sub ini_write(fromFolder As String, toFolder As String)

		Try
			Using writer1 = New StreamWriter(My.Application.Info.DirectoryPath & "\log.ini", False, System.Text.Encoding.GetEncoding(932))
				writer1.WriteLine(fromFolder & "," & toFolder & ",")
				'For Each st As ST_LOG In g_logs
				'writer1.WriteLine(st.fromFolder & "," & st.toFolder & "," & CStr(st.skipDel))
				'Next

			End Using
		Catch ex As Exception
		End Try

	End Sub


#End Region

End Module
