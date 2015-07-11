Imports System.DirectoryServices
Imports System.Net
Imports System.IO
Imports System.Timers



Public Class mainForm
    Public hostNames As List(Of String)
    Public ipAddress As List(Of String)
    Public pingTimer As Timer
    Public fn As String
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        initializePingTimer()
        Control.CheckForIllegalCrossThreadCalls = False
        readTable("")
    End Sub

    Public Sub initializePingTimer()
        pingTimer = New Timer(1000)
        pingTimer.AutoReset = True
        pingTimer.Enabled = True
        AddHandler pingTimer.Elapsed, AddressOf updatePings
        pingTimer.Start()
    End Sub


    Public Sub readTable(ByVal fileName As String)
        Dim fs As FileStream
        hostNames = New List(Of String)
        ipAddress = New List(Of String)

        If fileName = "" Then
            fileName = "data_table.rhc"
            fs = File.Open("data_table.rhc", FileMode.OpenOrCreate)
        Else
            fs = File.Open(fileName, FileMode.Open)
        End If
        fs.Close()

        fn = fileName

        Dim parts As String()
        Using reader As StreamReader = New StreamReader(fileName)
            Dim line As String
            line = reader.ReadLine()
            Do While (Not line Is Nothing And line <> "")
                parts = line.Split(",")
                If parts(0) <> "" Or parts(1) <> "" Then
                    hostNames.Add(parts(0))
                    ipAddress.Add(parts(1))
                End If
                line = reader.ReadLine()
            Loop
        End Using

        fillDataGridView()
        updateStatus()

    End Sub

    Public Sub fillDataGridView()
        Dim dataTable As New DataTable
        Dim ping As String
        dataTable.Columns.Add("Host Name")
        dataTable.Columns.Add("TCP/IP")
        dataTable.Columns.Add("Ping (ms)")

        Dim i As Integer = 0
        While i < hostNames.Count
            ping = -1
            If ipAddress(i) <> "" Then
                ping = GetPingMs(ipAddress(i))
            ElseIf hostNames(i) <> "" Then
                ping = GetPingMs(hostNames(i))
            End If
            dataTable.Rows.Add(hostNames(i), ipAddress(i), ping)
            System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
        End While

        DataGridView1.DataSource = dataTable
        DataGridView1.Refresh()
    End Sub

    Public Sub executeCommandPrompt(ByVal ipaddress As String)
        If ipaddress <> "" Then
            Dim commandString As String = "/c mstsc /v: " & ipaddress
            'If chkbox_fitScreen.CheckState = 1 Then
            '    commandString = commandString & " /f"
            '    commandString = commandString & " /multimon"
            'End If

            If chkbox_session.CheckState = 1 Then commandString = commandString & " /admin"
            Process.Start("cmd", commandString)
        End If
    End Sub

    Private Sub btn_Connect_Click(sender As Object, e As EventArgs) Handles btn_Connect.Click
        'MsgBox(DataGridView1.CurrentCell.Value.ToString)

        Dim FirstValue As Boolean = True
        Dim cell As DataGridViewCell
        Dim lastExecutedRow As Integer = -9999
        For Each cell In DataGridView1.SelectedCells
            Dim rowIndex As Integer = cell.OwningRow.Index
            Dim colName As String = cell.OwningColumn.DataPropertyName

            If lastExecutedRow <> rowIndex Then
                executeCommandPrompt(cell.Value)
                lastExecutedRow = rowIndex
            End If
        Next
    End Sub

    Private Sub DataGridView1_CellEndEdit(ByVal sender As Object, _
        ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView1.CellEndEdit

        ' Clear the row error in case the user presses ESC.   
        DataGridView1.Rows(e.RowIndex).ErrorText = String.Empty
        UpdateTable()
    End Sub

    Private Sub DataGridView1_UserDeletingRow( _
        ByVal sender As Object, _
        ByVal e As System.Windows.Forms. _
        DataGridViewRowCancelEventArgs) Handles DataGridView1.UserDeletingRow

        If (Not e.Row.IsNewRow) Then
            Dim response As DialogResult = _
            MessageBox.Show( _
            "Are you sure you want to delete this row?", _
            "Delete row?", _
            MessageBoxButtons.YesNo, _
            MessageBoxIcon.Question, _
            MessageBoxDefaultButton.Button2)
            If (response = DialogResult.No) Then
                e.Cancel = True
            End If
        End If
    End Sub
    Private Sub DataGridView1_UserDeletedRow(sender As Object, e As DataGridViewRowEventArgs) Handles DataGridView1.UserDeletedRow
        UpdateTable()
    End Sub

    Public Sub UpdateTable()
        Dim result As String = ""
        Dim sw As New System.IO.StreamWriter("data_table.rhc")

        'go through all rows
        For rowNumber As Integer = 0 To DataGridView1.Rows.Count - 2
            result = DataGridView1.Item(0, rowNumber).Value & ","
            result += DataGridView1.Item(1, rowNumber).Value
            If result <> "," Then
                sw.WriteLine(result)
            End If
        Next

        sw.Close()

    End Sub

    Public Sub ResetTable()
        Dim fs As FileStream = File.Create("data_table.rhc")
        fs.Close()
    End Sub

    Public Shared Function GetPingMs(ByRef hostNameOrAddress As String)
        Dim ping As New System.Net.NetworkInformation.Ping
        Dim result As Integer
        Try
            If hostNameOrAddress <> "" Then result = ping.Send(hostNameOrAddress).RoundtripTime
        Catch e As InvalidOperationException
            result = -1
        End Try

        Return result
    End Function

    Public Function updatePings(source As Object, e As ElapsedEventArgs)
        For rowNumber As Integer = 0 To DataGridView1.Rows.Count - 2
            If Not IsDBNull(DataGridView1.Item(1, rowNumber).Value) Then
                DataGridView1.Item(2, rowNumber).Value = GetPingMs(DataGridView1.Item(1, rowNumber).Value)
            ElseIf Not IsDBNull(DataGridView1.Item(0, rowNumber).Value) Then
                DataGridView1.Item(2, rowNumber).Value = GetPingMs(DataGridView1.Item(0, rowNumber).Value)
            End If
        Next
        Return 0
    End Function

    Private Sub CloseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem.Click
        Close()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        'MsgBox("Remote Hero" & vbNewLine & "AscendicaDevelopment" & vbNewLine & "2015")
        Dim frmAbout As New AboutBox
        frmAbout.ShowDialog(Me)
    End Sub

    Private Sub NewToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NewToolStripMenuItem.Click
        Dim response As DialogResult = _
            MessageBox.Show( _
            "Are you sure you want to create a new file?" & vbNewLine & "Doing so will overwrite your current configuration", _
            "Create a new file?", _
            MessageBoxButtons.YesNo, _
            MessageBoxIcon.Question, _
            MessageBoxDefaultButton.Button2)
        If (response = DialogResult.No) Then
            Exit Sub
        End If

        ResetTable()
        readTable("")

    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Dim ofd As New OpenFileDialog
        Dim response As DialogResult
        ofd.DefaultExt = ".rhc"
        ofd.InitialDirectory = Application.StartupPath()
        ofd.Filter = "Remote Hero Config (*.rhc)|*.rhc"
        ofd.FilterIndex = 1
        ofd.Multiselect = False
        response = ofd.ShowDialog()
        If response <> Windows.Forms.DialogResult.Cancel Then
            response = _
            MessageBox.Show( _
            "Are you sure you want to open a new file?" & vbNewLine & "Doing so will overwrite your current configuration", _
            "Open a new file?", _
            MessageBoxButtons.YesNo, _
            MessageBoxIcon.Question, _
            MessageBoxDefaultButton.Button2)
            If (response = DialogResult.No) Then
                Exit Sub
            End If
            ResetTable()
            readTable(ofd.FileName)
        End If
    End Sub

    Public Sub updateStatus()
        status_lbl.Text = "Status: " & fn & " loaded"
    End Sub

End Class
