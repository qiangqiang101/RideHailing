Imports System.IO
Imports System.Xml.Serialization
Imports GTA.Math

Public Structure LocationData

    Public ReadOnly Property Instance As LocationData
        Get
            Return ReadFromFile()
        End Get
    End Property

    <XmlIgnore>
    Public Property FileName() As String

    Public Places As List(Of Place)

    Public Sub New(_filename As String, place As List(Of Place))
        FileName = _filename
        Places = place
    End Sub

    Public Sub New(_filename As String)
        FileName = _filename
    End Sub

    Public Sub Save()
        Dim ser = New XmlSerializer(GetType(LocationData))
        Dim writer As TextWriter = New StreamWriter(FileName)
        ser.Serialize(writer, Me)
        writer.Close()
    End Sub

    Public Function ReadFromFile() As LocationData
        If Not File.Exists(FileName) Then
            Return New LocationData(FileName, Places)
        End If

        Try
            Dim ser = New XmlSerializer(GetType(LocationData))
            Dim reader As TextReader = New StreamReader(FileName)
            Dim instance = CType(ser.Deserialize(reader), LocationData)
            reader.Close()
            Return instance
        Catch ex As Exception
            Return New LocationData(FileName, Places)
        End Try
    End Function
End Structure

Public Structure Place
    Public Name As String
    Public Position As Vector3
    Public Popularity As Single

    Public Sub New(n As String, p As Vector3, pop As Single)
        Name = n
        Position = p
        Popularity = pop
    End Sub
End Structure