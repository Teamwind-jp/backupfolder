Imports System.ComponentModel
Imports System.IO

Public Class Form1

#Region "変数"

	'処理中フラグ
	Private factivate As Boolean = False

	'中断問い合わせフラグ
	Private fabort As Boolean = False

	'コピーフォルダ
	Private sourcefolder As String = ""
	Private destfolder As String = ""

	'処理数カウンタ
	Private nFileCount As Long = 0

	' ノード用
	Private nLevel As Integer = 0   '階層

	'タブごとの処理結果ノード全体オブジェクト
	Private arrDirNode As TreeNode

	'各レベルの現在のノード windowsは250が制限値なのでとりあえず余分に切った
	'処理中のディレクトリ処理で深さに応じて親ノードを特定するためのワーク
	Private arrDirNodeLevel(300) As TreeNode 'ノード 

	Private lastnode As TreeNode

#End Region

#Region "デリゲート共用"

	'処理中フラグ
	Public Function _dlg_getfactivate() As Boolean
		If Me.InvokeRequired Then
			Return CType(Me.Invoke(New Func(Of Boolean)(AddressOf _dlg_getfactivate)), Boolean)
		Else
			Return factivate
		End If
	End Function

	Public Sub _dlg_setfactivate(value As Boolean)
		If Me.InvokeRequired Then
			Me.Invoke(New Action(Of Boolean)(AddressOf _dlg_setfactivate), value)
		Else
			factivate = value
		End If
	End Sub


	'中断問い合わせフラグ
	Public Function _dlg_getfabort() As Boolean
		If Me.InvokeRequired Then
			Return CType(Me.Invoke(New Func(Of Boolean)(AddressOf _dlg_getfabort)), Boolean)
		Else
			Return fabort
		End If
	End Function

	Public Sub _dlg_setfabort(value As Boolean)
		If Me.InvokeRequired Then
			Me.Invoke(New Action(Of Boolean)(AddressOf _dlg_setfabort), value)
		Else
			fabort = value
		End If
	End Sub

	'対象フォルダ
	Public Function _dlg_getsourcefolder() As String
		If Me.InvokeRequired Then
			Return CType(Me.Invoke(New Func(Of String)(AddressOf _dlg_getsourcefolder)), String)
		Else
			Return sourcefolder
		End If
	End Function
	Public Sub _dlg_setsourcefolder(value As String)
		If Me.InvokeRequired Then
			Me.Invoke(New Action(Of String)(AddressOf _dlg_setsourcefolder), value)
		Else
			sourcefolder = value
		End If
	End Sub

	Public Function _dlg_getdestfolder() As String
		If Me.InvokeRequired Then
			Return CType(Me.Invoke(New Func(Of String)(AddressOf _dlg_getdestfolder)), String)
		Else
			Return destfolder
		End If
	End Function

	Public Sub _dlg_setdestfolder(value As String)
		If Me.InvokeRequired Then
			Me.Invoke(New Action(Of String)(AddressOf _dlg_setdestfolder), value)
		Else
			destfolder = value
		End If
	End Sub

	'処理ファイル数取得
	Public Function _dlg_getFileCount() As Long
		If Me.InvokeRequired Then
			Return CType(Me.Invoke(New Func(Of Long)(AddressOf _dlg_getFileCount)), Long)
		Else
			Return nFileCount
		End If
	End Function

	'処理ファイル数クリア
	Private Sub _dlg_clearFileCount()
		If Me.InvokeRequired Then
			'デリゲート呼び出し
			Me.Invoke(New Action(AddressOf _dlg_clearFileCount))
		Else
			nFileCount = 0
		End If
	End Sub

	'処理ファイル数up
	Public Sub _dlg_addFileCount()
		If Me.InvokeRequired Then
			'デリゲート呼び出し
			Me.Invoke(New Action(AddressOf _dlg_addFileCount))
		Else
			nFileCount += 1
		End If
	End Sub

	'button text
	Private Sub _dlg_setButtonText(value As String)
		If Button1.InvokeRequired Then
			Button1.Invoke(New Action(Of String)(AddressOf _dlg_setButtonText), value)
		Else
			Button1.Text = value
		End If
	End Sub

	'info text
	Private Sub _dlg_seInfoText(value As String)
		If txtInfo.InvokeRequired Then
			txtInfo.Invoke(New Action(Of String)(AddressOf _dlg_seInfoText), value)
		Else
			txtInfo.Text = value
		End If
	End Sub

	'TreeNode クリア
	Private Sub _dlg_clearTreeNode()
		If Me.InvokeRequired Then
			'デリゲート呼び出し
			Me.Invoke(New Action(AddressOf _dlg_clearTreeNode))
		Else
			TreeView1.Nodes.Clear()
		End If
	End Sub

	'TreeNode add
	Private Sub _dlg_addTreeNode(sz As String, full As String)
		If Me.InvokeRequired Then
			'デリゲート呼び出し
			Me.Invoke(New Action(Of String, String)(AddressOf _dlg_addTreeNode), sz, full)
		Else

			'ノード追加処理
			Dim newNode As New TreeNode
			newNode.Text = sz

			lastnode = newNode

			arrDirNodeLevel(nLevel) = newNode

			'現在のレベルの親ノードに追加する
			If nLevel = 0 Then
				'ルートノードに追加
				TreeView1.Nodes.Add(newNode)
			Else

				If nLevel > 0 Then
					'子ノード　孫ノード.... なので1つ親に追加する
					arrDirNodeLevel(nLevel - 1).Nodes.Add(newNode)
				End If

			End If
			TreeView1.ExpandAll()
		End If

	End Sub

	'treenode update　コピー数表示
	Private Sub _dlg_upTreeNode(cnt As Integer)
		If Me.InvokeRequired Then
			'デリゲート呼び出し
			Me.Invoke(New Action(Of Integer)(AddressOf _dlg_upTreeNode), cnt)
		Else
			'ノード追加処理
			If cnt > 0 Then
				lastnode.Text &= "<" & cnt.ToString & ">"
				lastnode.ForeColor = Color.MediumBlue
			End If
		End If
	End Sub

	'ノードレベルカウンタ変更
	Public Sub _dlg_updownNodeLevel(value As Integer)
		If Me.InvokeRequired Then
			Me.Invoke(New Action(Of Integer)(AddressOf _dlg_updownNodeLevel), value)
		Else
			nLevel += value
		End If
	End Sub

#End Region

#Region "on load"

	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

		'前回値あれば読み込み
		ini_read()

		If g_logs.Count > 0 Then
			TextBox1.Text = g_logs.Item(0).fromFolder
			TextBox2.Text = g_logs.Item(0).toFolder
		End If

	End Sub


#End Region

#Region "copy本体"

	Private Function CopyFolderFile(ByVal frompath As String, ByVal topath As String) As Integer

		Dim bcopy As Boolean
		Dim szToPath, szFromPath As String          'これは先頭で確定しているので　使い回しは厳禁
		Dim szToFile As String
		Dim dateTo As Date
		Dim dateFrom As Date
		Dim i As Integer

		Dim rc As Integer


		'出力明細を一行でもはき出せばon
		Dim bout As Boolean = False
		Dim bout2 As Boolean = False
		Dim targetpath As String

		'On Error GoTo copyerror

		'コピー開始を明示する
		targetpath = frompath + " → " + topath

		'---------------------------------------------------------------------------------------------------
		'処理フォルダ準備

		'転送先パスの最後の\を除く
		If Microsoft.VisualBasic.Right(topath, 1) = "\" Then
			szToPath = Microsoft.VisualBasic.Left(topath, Len(topath) - 1)
		Else
			szToPath = topath
		End If

		'転送元パスの最後の\を除く
		If Microsoft.VisualBasic.Right(frompath, 1) = "\" Then
			szFromPath = Microsoft.VisualBasic.Left(frompath, Len(frompath) - 1)
		Else
			szFromPath = frompath
		End If

		'転送元tree viewノード追加
		_dlg_addTreeNode(getLastPath(szFromPath), szFromPath)

		'転送先フォルダが無ければ生成する
		Try
			If System.IO.Directory.Exists(szToPath & "\") Then
			Else
				'NG
				'無いので作る
				System.IO.Directory.CreateDirectory(szToPath & "\")
				If System.IO.Directory.Exists(szToPath & "\") Then
				Else
					'生成エラー
					'次のフォルダへ
					Return 0
				End If
			End If
		Catch ex As Exception
			'生成エラー
			'次のフォルダへ
			Return 0
		End Try

		'---------------------------------------------------------------------------------------------------
		'ファイルをコピーする
		Dim ncopy, nskip, ndelete As Integer
		ncopy = 0
		ndelete = 0
		nskip = 0

		Try
			'転送元の一覧を取得する
			Dim strFiles() As String = System.IO.Directory.GetFiles(szFromPath + "\", "*.*")
			'1ファイルずつコピーする
			For Each strFile In strFiles

				'その前に中断指示チェック
				If _dlg_getfabort() = True Then

					'中断
					Using topMostForm As New Form()
						topMostForm.TopMost = True
						topMostForm.StartPosition = FormStartPosition.Manual
						topMostForm.Location = New Drawing.Point(-2000, -2000) ' 画面外に配置
						topMostForm.Show()
						topMostForm.Focus()
						If MessageBox.Show("中断しますか？", Me.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = Windows.Forms.DialogResult.Yes Then
							'コピー終了
							_dlg_setfactivate(False)
						End If
					End Using

					'問い合わせ終了
					_dlg_setfabort(False)
				End If

				'処理中断チェック
				If _dlg_getfactivate() = False Then
					'中断
					Return -999
				End If

				'ファイル名のみを抽出
				Dim di As New System.IO.DirectoryInfo(strFile)
				szToFile = di.Name

				'コピー対象かをここで判定する
				bcopy = True

				'同名ファイルがある場合は、日付判定　新しい場合だけコピーする
				If System.IO.File.Exists(szToPath + "\" + szToFile) Then
					'有るので更新日をチェックする
					dateFrom = FileDateTime(strFile)
					dateTo = FileDateTime(szToPath + "\" + szToFile)

					'日付を比較してdateFromがdateToより新しければコピー
					If Date.Compare(dateFrom, dateTo) <= 0 Then
						'古いのでコピーしない
						bcopy = False
					End If
				End If

				'その他判定はここで


				'対象外はスキップ
				If bcopy = False Then
					Continue For
				End If

				'コピー
				Dim fromfull As String = szFromPath + "\" + szToFile

				_dlg_seInfoText(fromfull)
				Application.DoEvents()

				Try
					'コピー
					System.IO.File.Copy(fromfull, szToPath + "\" + szToFile, True)
					'全体数カウントアップ
					_dlg_addFileCount()
					'当該フォルダコピー数カウントアップ
					ncopy += 1

				Catch ex As Exception
					'ロック中かも
				End Try

			Next
		Catch ex As Exception
			'次のフォルダへ
			Return 0
		End Try

		If ncopy > 0 Then
			'ノードにコピー数反映
			_dlg_upTreeNode(ncopy)
		End If


		'サブフォルダを処理する
		Try
			Dim strFolderPath() As String = System.IO.Directory.GetDirectories(szFromPath + "\")

			'1階層下の全フォルダを1つずつ処理　
			For Each strFile In strFolderPath

				Try
					'隠しファイル/フォルダはスキップ
					Dim attributes As FileAttributes = File.GetAttributes(strFile)
					If (attributes And FileAttributes.Hidden) <> FileAttributes.Hidden Then
						'転送先を生成する
						Dim sztemp = szToPath
						For i = Len(strFile) To 1 Step -1
							If Microsoft.VisualBasic.Mid(strFile, i, 1) = "\" Then
								'区切りを発見
								sztemp += Microsoft.VisualBasic.Mid(strFile, i)
								Exit For
							End If
						Next

						'1階層下へ宣言
						_dlg_updownNodeLevel(1)

						'リエントラント処理
						rc = CopyFolderFile(strFile, sztemp)

						'1階層戻す
						_dlg_updownNodeLevel(-1)

						If rc <> 0 Then
							'中断エラーは終了
							Return rc
						End If
						Application.DoEvents()
					End If
				Catch ex As Exception
				End Try

			Next
		Catch ex As Exception
			'次のフォルダへ
			Return 0
		End Try

		Return 0



	End Function




#End Region

#Region "コピー処理　バックグラウンド"

	Private Function RealTimeControl(ByVal worker As System.ComponentModel.BackgroundWorker, ByVal e As System.ComponentModel.DoWorkEventArgs) As Long

		'処理ファイル数
		_dlg_clearFileCount()

		'--------------------------------------------------------------------------
		'コピー開始

		'処理中宣言
		_dlg_setfactivate(True)

		_dlg_setButtonText("中断")

		'テキストボックスクリア
		_dlg_seInfoText("")

		'ツリービュークリア
		_dlg_clearTreeNode()

		'開始
		Dim rc = CopyFolderFile(_dlg_getsourcefolder(), _dlg_getdestfolder())

		'結果
		_dlg_seInfoText("完了　" & _dlg_getFileCount().ToString & "個のファイルをコピーしました。")

		'宣言解除
		_dlg_setfactivate(False)

		'ボタンテキスト戻す
		_dlg_setButtonText("実行")


		Return 0

	End Function


	Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
		' BackgroundWorkerの取得(スレッドを作成したオブジェクト)
		Dim objWorker As System.ComponentModel.BackgroundWorker = CType(sender, System.ComponentModel.BackgroundWorker)
		'ここから別スレッド
		e.Result = RealTimeControl(objWorker, e)
	End Sub

	'go or stop button
	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

		'実行中なら中断問い合わせ
		If _dlg_getfactivate() = True Then
			If _dlg_getfabort() = True Then
				'すでに中断問い合わせ　　bye
				Return
			End If
			'すでに中断問い合わせする
			_dlg_setfabort(True)

			'bye
			Return
		End If

		'folderチェック
		Dim sfolder As String = TextBox1.Text.Trim
		Dim dfolder As String = TextBox2.Text.Trim
		If sfolder = "" Or dfolder = "" Then
			MessageBox.Show("コピー元、コピー先フォルダを指定してください。", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
			Return
		End If

		'元フォルダチェック
		If Not System.IO.Directory.Exists(sfolder) Then
			MessageBox.Show("コピー元フォルダが存在しません。", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
			Return
		End If


		'先フォルダチェック

		'システムフォルダはダメ
		Dim systemRoot As String = Environment.GetEnvironmentVariable("SystemRoot")

		If dfolder.ToLower.IndexOf(systemRoot.ToLower) >= 0 Then
			MessageBox.Show("システムフォルダへのコピーはできません。", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
			Return
		End If

		'コピー元フォルダはダメ


		If isSubdirectory(sfolder, dfolder) = True Then
			MessageBox.Show("エラー　コピー先フォルダがコピー元フォルダの中にあります。", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
			Return
		End If

		'システムドライブと同じドライブは警告
		If dfolder.Substring(0, 1).ToLower = systemRoot.Substring(0, 1).ToLower Then
			If MessageBox.Show("システムのあるドライブへコピーしようとしています。復旧出来ない致命傷を与える可能性があります。実行しますか？", Me.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) <> Windows.Forms.DialogResult.Yes Then
				Return
			End If
		End If

		'コピー先フォルダが存在しない場合は作成
		If Not System.IO.Directory.Exists(dfolder) Then
			Try
				System.IO.Directory.CreateDirectory(dfolder)
			Catch ex As Exception
				MessageBox.Show("コピー先フォルダの作成に失敗しました。", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
				Return
			End Try
		End If

		'データ退避
		_dlg_setsourcefolder(sfolder)
		_dlg_setdestfolder(dfolder)

		'開始確認
		If MessageBox.Show("コピーを開始しますか？", Me.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> Windows.Forms.DialogResult.Yes Then
			Return
		End If

		'設定退避
		ini_write(sfolder, dfolder)

		'実行指示
		BackgroundWorker1.RunWorkerAsync()

	End Sub



#End Region

#Region "D & D "

	Private Sub TextBox1_DragDrop(sender As Object, e As DragEventArgs) Handles TextBox1.DragDrop

		Dim strFileName As String() = CType(e.Data.GetData(DataFormats.FileDrop, False), String())

		If System.IO.Directory.Exists(strFileName(0).ToString) = True Then
			Me.TextBox1.Text = strFileName(0).ToString
		End If

	End Sub

	Private Sub TextBox1_DragEnter(sender As Object, e As DragEventArgs) Handles TextBox1.DragEnter

		If e.Data.GetDataPresent(DataFormats.FileDrop) = True Then
			e.Effect = DragDropEffects.Copy
		Else
			e.Effect = DragDropEffects.None
		End If

	End Sub


	Private Sub TextBox2_DragDrop(sender As Object, e As DragEventArgs) Handles TextBox2.DragDrop

		Dim strFileName As String() = CType(e.Data.GetData(DataFormats.FileDrop, False), String())

		If System.IO.Directory.Exists(strFileName(0).ToString) = True Then
			Me.TextBox2.Text = strFileName(0).ToString
		End If
	End Sub

	Private Sub TextBox2_DragEnter(sender As Object, e As DragEventArgs) Handles TextBox2.DragEnter

		If e.Data.GetDataPresent(DataFormats.FileDrop) = True Then
			e.Effect = DragDropEffects.Copy
		Else
			e.Effect = DragDropEffects.None
		End If

	End Sub

#End Region

#Region "サブ関数系"

	'フォルダ階層の属性を返す  再帰処理
	Private Function getFoldersAttributes(path As String) As FileAttributes

		Dim rc As FileAttributes = CType(0, FileAttributes)

		Try
			' 現在のフォルダの属性を取得
			rc = File.GetAttributes(path)

			' 親を再帰的に処理
			Dim parentDir As DirectoryInfo = Directory.GetParent(path)
			If parentDir IsNot Nothing Then
				rc = rc Or getFoldersAttributes(parentDir.FullName)
			End If

		Catch ex As UnauthorizedAccessException
			' スキップ
		Catch ex As PathTooLongException
			' スキップ
		Catch ex As Exception
			' その他
		End Try

		Return rc
	End Function


	'最終フォルダ名を取得
	Private Function getLastPath(sz As String) As String

		Dim arr1() As String = sz.Split("\")

		Return arr1(arr1.Length - 1)

	End Function


	Function isSubdirectory(parentPath As String, childPath As String) As Boolean
		Try
			' 絶対パスに変換（末尾に区切り文字を追加）
			Dim parentFull As String = Path.GetFullPath(parentPath.Trim())
			If parentFull.Length > 3 Then
				parentFull &= Path.DirectorySeparatorChar
			End If

			Dim childFull As String = Path.GetFullPath(childPath.Trim())
			If childFull.Length > 3 Then
				childFull &= Path.DirectorySeparatorChar
			End If

			' 大文字小文字を無視して比較（Windows想定）
			Return childFull.StartsWith(parentFull, StringComparison.OrdinalIgnoreCase)
		Catch ex As Exception
			' パスが不正な場合は False
			Return False
		End Try
	End Function

#End Region


End Class
