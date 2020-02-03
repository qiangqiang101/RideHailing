Public NotInheritable Class logger

    Public Shared Sub Log(message As Object)
        IO.File.AppendAllText(".\PointOfInterest.txt", message & Environment.NewLine)
    End Sub

End Class