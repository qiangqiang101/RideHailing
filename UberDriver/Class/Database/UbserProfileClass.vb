Public Class UbserProfileClass

    Public Earnings As List(Of Earning)
    Public Comments As List(Of Comment)
    Public Vehicles As List(Of UberVehicle)

    Public Sub New(earning As List(Of Earning), comment As List(Of Comment), veh As List(Of UberVehicle))
        Earnings = earning
        Comments = comment
        Vehicles = veh
    End Sub

End Class
