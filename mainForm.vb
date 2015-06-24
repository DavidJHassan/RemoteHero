Imports System.DirectoryServices
Imports System.Net
Imports System.IO

Public Class mainForm
    Public hostNames As New List(Of String)
    Public ipAddress As New List(Of String)
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        readTable()
        fillDataGridView()
        'UpdateTable()
    End Sub

    Public Sub readTable()
        Dim fs As FileStream = File.Open("data_table.ini", FileMode.OpenOrCreate)
        fs.Close()

        Dim parts As String()
        Using reader As StreamReader = New StreamReader("data_table.ini")
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
    End Sub

    Public Sub fillDataGridView()
        Dim dataTable As New DataTable
        dataTable.Columns.Add("Host Name")
        dataTable.Columns.Add("TCP/IP")
        dataTable.Columns.Add("Ping (ms)")

        Dim i As Integer = 0
        While i < hostNames.Count
            dataTable.Rows.Add(hostNames(i), ipAddress(i), "")
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
        Dim sw As New System.IO.StreamWriter("data_table.ini")

        'go through all rows
        For rowNumber As Integer = 0 To DataGridView1.Rows.Count - 1
            result = DataGridView1.Item(0, rowNumber).Value & ","
            result += DataGridView1.Item(1, rowNumber).Value
            If result <> "," Then
                sw.WriteLine(result)
            End If
        Next

        sw.Close()

    End Sub

    Public Sub ResetTable()
        File.Create("data_table.ini")
    End Sub

End Class
