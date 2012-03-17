Class MainWindow

    Public Shared ParseLog As New RoutedCommand

    Private cgEntries As New System.ComponentModel.BindingList(Of WolfLight.SWTORCombatLog.LogEntry)



    Public Sub CanAlwaysExecute(ByVal sender As Object, ByVal e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub

    Public Sub ParseLog_Execute(ByVal sender As Object, ByVal e As ExecutedRoutedEventArgs)
        Dim filename As String = GetFilename()


        If String.IsNullOrEmpty(filename) = False Then
            Try
                Dim strm As New System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read)
                Dim strmReader As New System.IO.StreamReader(strm)
                Dim entries As New List(Of WolfLight.SWTORCombatLog.LogEntry)

                cgEntries.Clear()

                WolfLight.SWTORCombatLog.LogParser.ParseStream(strmReader, entries)

                For i As Integer = 0 To entries.Count - 1
                    cgEntries.Add(entries(i))
                Next i

            Catch ex As System.IO.FileNotFoundException
                Stop
            End Try
        End If
    End Sub

    Private Function GetFilename() As String
        Dim ofd As New Microsoft.Win32.OpenFileDialog()


        ofd.AddExtension = True
        ofd.DefaultExt = "txt"
        ofd.CheckFileExists = True
        ofd.CheckPathExists = True
        ofd.Multiselect = False

        If ofd.ShowDialog Then
            Return ofd.FileName
        Else
            Return Nothing
        End If
    End Function

    Private Sub MainWindow_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        dgEntries.ItemsSource = cgEntries
    End Sub
End Class
