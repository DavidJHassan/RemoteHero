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
    End Sub

    Public Sub readTable()
        'Dim dataTable = File.Open("data_table.ini", FileMode.OpenOrCreate)
        Dim parts As String()
        Using reader As StreamReader = New StreamReader("data_table.ini")
            Dim line As String
            line = reader.ReadLine()
            Do While (Not line Is Nothing And line <> "")
                parts = line.Split(",")
                If parts(0) <> "" And parts(1) <> "" Then
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

        Dim i As Integer = 0
        While i < hostNames.Count
            dataTable.Rows.Add(hostNames(i), ipAddress(i))
            System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
        End While

        DataGridView1.DataSource = dataTable
        DataGridView1.Refresh()
    End Sub

    Public Sub executeCommandPrompt(ByVal ipaddress As String)
        Dim p As Process = New Process()
        'Process.Start("cmd", "/k mstsc /v: " & ipaddress & " /admin")
        Process.Start("cmd", "/k echo " & ipaddress)
    End Sub


    'Dim de As New DirectoryEntry()
    'de.Path = "LDAP://DC=BOWATER"


    'Try
    '    For Each d As DirectoryEntry In de.Children()
    '        MsgBox(d.Name)
    '    Next
    'Catch ex As Exception
    '    MsgBox(ex.Message)
    'End Try


    'Dim strHostName As New String("")
    '' Getting Ip address of local machine...
    '' First get the host name of local machine.
    'strHostName = Dns.GetHostName()

    '' Then using host name, get the IP address list..
    'Dim ipEntry As IPHostEntry = Dns.GetHostByName(strHostName)
    'Dim addr As IPAddress() = ipEntry.AddressList

    'Dim i As Integer = 0
    'Dim dataTable As New DataTable
    'dataTable.Columns.Add("Host Name")
    'dataTable.Columns.Add("TCP/IP")

    'While i < addr.Length
    '    dataTable.Rows.Add(Dns.GetHostByAddress(addr(i).ToString()).HostName, addr(i).ToString())
    '    'hostName.Add()
    '    'ip.Add()
    '    System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
    'End While
    'DataGridView1.DataSource = dataTable
    'DataGridView1.Refresh()


    Private Sub btn_Connect_Click(sender As Object, e As EventArgs) Handles btn_Connect.Click
        'MsgBox(DataGridView1.CurrentCell.Value.ToString)

        Dim FirstValue As Boolean = True
        Dim cell As DataGridViewCell
        Dim lastExecutedRow As Integer = -9999
        For Each cell In DataGridView1.SelectedCells
            Dim rowIndex As Integer = cell.OwningRow.Index
            Dim colName As String = cell.OwningColumn.DataPropertyName

            If lastExecutedRow <> rowIndex Then
                If cell.Value <> "" Then
                    executeCommandPrompt(cell.Value)
                End If
                lastExecutedRow = rowIndex
            End If
        Next
    End Sub
End Class



