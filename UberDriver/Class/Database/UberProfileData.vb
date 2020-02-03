Imports System.Xml.Serialization
Imports GTA.Math
Imports System.IO
Imports GTA

Public Structure UberProfileData

    Public ReadOnly Property Instance As UberProfileData
        Get
            Return ReadFromFile()
        End Get
    End Property

    <XmlIgnore>
    Public Property ProfileFileName() As String
    <XmlIgnore>
    Public ReadOnly Property Trips() As Integer
        Get
            On Error Resume Next
            Return Earnings.Sum(Function(x) x.TripCount)
        End Get
    End Property
    <XmlIgnore>
    Public ReadOnly Property Earning() As Single
        Get
            On Error Resume Next
            Return Earnings.Sum(Function(x) x.Earnings)
        End Get
    End Property
    <XmlIgnore>
    Public ReadOnly Property Rating() As Single
        Get
            On Error Resume Next
            Return Comments.Sum(Function(x) x.Ratings)
        End Get
    End Property

    Public Earnings As List(Of Earning)
    Public Comments As List(Of Comment)
    Public Vehicles As List(Of UberVehicle)

    Public Sub New(_filename As String, earning As List(Of Earning), comment As List(Of Comment), veh As List(Of UberVehicle))
        ProfileFileName = _filename
        Earnings = earning
        Comments = comment
        Vehicles = veh
    End Sub

    Public Sub New(_filename As String)
        ProfileFileName = _filename
    End Sub

    Public Sub Save()
        Dim ser = New XmlSerializer(GetType(UberProfileData))
        Dim writer As TextWriter = New StreamWriter(ProfileFileName)
        ser.Serialize(writer, Me)
        writer.Close()
    End Sub

    Public Function ReadFromFile() As UberProfileData
        If Not File.Exists(ProfileFileName) Then
            Return New UberProfileData(ProfileFileName, Earnings, Comments, Vehicles)
        End If

        Try
            Dim ser = New XmlSerializer(GetType(UberProfileData))
            Dim reader As TextReader = New StreamReader(ProfileFileName)
            Dim instance = CType(ser.Deserialize(reader), UberProfileData)
            reader.Close()
            Return instance
        Catch ex As Exception
            Return New UberProfileData(ProfileFileName, Earnings, Comments, Vehicles)
        End Try
    End Function
End Structure

Public Structure Comment
    Public Compliment As ComplimentType
    Public FromUser As String
    Public Comments As String
    Public Ratings As Single

    Public Sub New(comp As ComplimentType, user As String, comment As String, rate As Single)
        Compliment = comp
        FromUser = user
        Comments = comment
        Ratings = rate
    End Sub
End Structure

Public Enum ComplimentType
    ExcellentService
    ExpertNavigation
    AboveAndBeyond
    AwesomeMusic
    NeatAndTidy
    CoolCar
End Enum

Public Structure Earning
    Public [Date] As String
    Public Trips As List(Of Trip)
    Public TripCount As Integer
    Public Earnings As Single

    Public Sub New(_date As String, trip As List(Of Trip))
        [Date] = _date
        Trips = trip
        Earnings = trip.Sum(Function(x) x.YourEarnings)
        TripCount = trip.Count
    End Sub
End Structure

Public Structure Trip
    Public TimeRequested As String
    Public [Date] As String
    Public YourEarnings As Single
    Public Type As UberType
    Public Fare As Single
    Public UberFee As Single

    Public Sub New(time As String, _date As String, utype As UberType, _fare As Single, ufee As Single)
        TimeRequested = time
        [Date] = _date
        Type = utype
        Fare = _fare
        UberFee = ufee
        YourEarnings = _fare - ufee
    End Sub
End Structure

Public Enum UberType
    NotSuitable
    UberX
    UberXL
    UberSelect
    UberBLACK
End Enum

Public Structure UberVehicle
    Public Name As String
    Public Brand As String
    Public Hash As Integer
    Public Color As VehicleColor
    Public VehicleClass As VehicleClass
    Public Type As UberType
    Public Plate As String

    Public Sub New(_name As String, _brand As String, _plate As String, _hash As String, col As VehicleColor, vehclass As VehicleClass, _type As UberType)
        Name = _name
        Brand = _brand
        Plate = _plate
        Hash = _hash
        Color = col
        VehicleClass = vehclass
        Type = _type
    End Sub

    Public Sub New(vehicle As Vehicle)
        Name = vehicle.FriendlyName
        Brand = vehicle.Brand
        Hash = vehicle.Model.Hash
        Color = vehicle.PrimaryColor
        VehicleClass = vehicle.ClassType
        Type = vehicle.UberType
    End Sub
End Structure