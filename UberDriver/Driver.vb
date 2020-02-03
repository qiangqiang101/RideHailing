Option Explicit On

Imports System.Drawing
Imports System.IO
Imports System.Media
Imports System.Windows.Forms
Imports GTA
Imports GTA.Math
Imports INMNativeUI

Public Class Driver
    Inherits Script

    ReadOnly _scaleform As New Scaleform("instructional_buttons")
    Dim JobAvailable As Boolean = False
    Dim HasSecondaryJob As Boolean = False

    Dim Rectangle = New UIResRectangle()
    Dim TodaysEarning As Single = 0F
    Dim Ticking As Integer = 500
    Dim Online As Boolean = False
    Dim CurrentPassenger As Ped = Nothing, CurrentDestination As Vector3 = Vector3.Zero
    Dim PickupLocation, DropoffLocation As Vector3
    Dim PingPlayer As New SoundPlayer() With {.Tag = "NotPlaying"}
    Dim Timeout As Integer = 0
    Dim IgnoreCount As Integer = 0

    Dim CurrentVehicle As UberVehicle = Nothing

    Dim WithEvents MainMenu, VehicleMenu, EarningsMenu, TripsMenu As UIMenu
    Dim ItemLastName, ItemTrips, itemRating, ItemMyVehicles, ItemEarnings, ItemComment, ItemOnline As UIMenuItem
    Dim _MenuPool As MenuPool
    Dim TripStarted As Boolean = False
    Dim DestBlip As Blip = Nothing

    Public Sub New()
        _MenuPool = New MenuPool()

        CreateMainMenu()
        CreateVehicleMenu()
        CreateEarningsMenu()
        CreateTripsMenu()

        If File.Exists(POIxml) Then
            Dim locationData As LocationData = New LocationData(POIxml).Instance()
            Locations = New LocationClass(locationData.Places)
            If Not Locations.Places.Count = 0 Then
                For Each Loca As Place In Locations.Places
                    Places.Add(Loca)
                Next
            End If
        End If
    End Sub

    Private Sub DrawInstructionalButton()
        GTA.Control.FrontendAccept.Disable
        GTA.Control.PhoneCancel.Disable
        _scaleform.Render2D()
        _scaleform.CallFunction("CLEAR_ALL")
        _scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0)
        _scaleform.CallFunction("CREATE_CONTAINER")
        _scaleform.CallFunction("SET_DATA_SLOT", 0, GET_CONTROL_INSTRUCTIONAL_BUTTON(GTA.Control.PhoneCancel), "No Thanks")
        _scaleform.CallFunction("SET_DATA_SLOT", 1, GET_CONTROL_INSTRUCTIONAL_BUTTON(GTA.Control.FrontendAccept), "Accept Job")
        _scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1)
    End Sub

    Private Sub DrawInstructionalButtonStartTrip()
        GTA.Control.FrontendAccept.Disable
        GTA.Control.PhoneCancel.Disable
        _scaleform.Render2D()
        _scaleform.CallFunction("CLEAR_ALL")
        _scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0)
        _scaleform.CallFunction("CREATE_CONTAINER")
        _scaleform.CallFunction("SET_DATA_SLOT", 0, GET_CONTROL_INSTRUCTIONAL_BUTTON(GTA.Control.PhoneCancel), "Cancel Trip")
        _scaleform.CallFunction("SET_DATA_SLOT", 1, GET_CONTROL_INSTRUCTIONAL_BUTTON(GTA.Control.FrontendAccept), "Start Trip")
        _scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1)
    End Sub

    Private Sub CreateMainMenu()
        MainMenu = New UIMenu("", "UBER", New Point(0, -107))
        MainMenu.SetBannerType(Rectangle)
        MainMenu.MouseEdgeEnabled = False
        _MenuPool.Add(MainMenu)
        Dim xmlFile As String = Path.GetFullPath($".\scripts\UberDriver\UserProfiles\{Game.Player.Character.Name}.xml")
        If Not File.Exists(xmlFile) Then
            CurrentProfile = New UberProfileData(xmlFile).Instance
            CurrentProfile.Save()
        End If
        CurrentProfile = New UberProfileData(xmlFile).Instance
        ItemLastName = New UIMenuItem("Name") : ItemLastName.SetRightLabel(Game.Player.Character.Name) : MainMenu.AddItem(ItemLastName)
        ItemMyVehicles = New UIMenuItem("My Vehicles", "Select your vehicles.") : MainMenu.AddItem(ItemMyVehicles)
        ItemTrips = New UIMenuItem("Trips", "Trips you completed today.") : ItemTrips.SetRightLabel(CurrentProfile.Trips) : MainMenu.AddItem(ItemTrips)
        itemRating = New UIMenuItem("Ratings", "Your overall ratings.") : itemRating.SetRightLabel(CurrentProfile.Rating.ToString("0.00")) : MainMenu.AddItem(itemRating)
        ItemEarnings = New UIMenuItem("Earnings", "Show your past Earnings.") : ItemEarnings.SetRightLabel($"${CurrentProfile.Earning.ToString("0.00")}") : MainMenu.AddItem(ItemEarnings)
        ItemComment = New UIMenuItem("Comments", "Show your passenger's comments and compliments.") : MainMenu.AddItem(ItemComment)
        Select Case Online
            Case True
                ItemOnline = New UIMenuItem("Offline", "End your Job.") : MainMenu.AddItem(ItemOnline)
            Case False
                ItemOnline = New UIMenuItem("Online", "Start your Job.") : MainMenu.AddItem(ItemOnline)
        End Select
        MainMenu.RefreshIndex()
    End Sub

    Private Sub RefreshMainMenu()
        Dim xmlFile As String = Path.GetFullPath($".\scripts\UberDriver\UserProfiles\{Game.Player.Character.Name}.xml")
        Dim cp As UberProfileData = New UberProfileData(xmlFile)
        cp.ReadFromFile()
        CurrentProfile = cp.Instance
        MainMenu.MenuItems.Clear()
        ItemLastName = New UIMenuItem("Name") : ItemLastName.SetRightLabel(Game.Player.Character.Name) : MainMenu.AddItem(ItemLastName)
        ItemMyVehicles = New UIMenuItem("My Vehicles", "Select your vehicles.")
        With ItemMyVehicles
            .SetRightLabel(CurrentVehicle.Name)
        End With
        MainMenu.AddItem(ItemMyVehicles)
        ItemTrips = New UIMenuItem("Trips", "Trips you completed today.") : ItemTrips.SetRightLabel(cp.Trips) : MainMenu.AddItem(ItemTrips)
        itemRating = New UIMenuItem("Ratings", "Your overall ratings.") : itemRating.SetRightLabel(cp.Rating.ToString("0.00")) : MainMenu.AddItem(itemRating)
        ItemEarnings = New UIMenuItem("Earnings", "Show your past Earnings.") : ItemEarnings.SetRightLabel($"${cp.Earning.ToString("0.00")}") : MainMenu.AddItem(ItemEarnings)
        ItemComment = New UIMenuItem("Comments", "Show your passenger's comments and compliments.") : MainMenu.AddItem(ItemComment)
        Select Case Online
            Case True
                ItemOnline = New UIMenuItem("Offline", "End your Job.") : MainMenu.AddItem(ItemOnline)
            Case False
                ItemOnline = New UIMenuItem("Online", "Start your Job.") : MainMenu.AddItem(ItemOnline)
        End Select
        MainMenu.RefreshIndex()
    End Sub

    Private Sub MainMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles MainMenu.OnItemSelect
        Dim c As Ped = Game.Player.Character
        Dim v As Vehicle = Game.Player.Character.LastVehicle

        Select Case selectedItem.Text
            Case ItemOnline.Text
                If ItemOnline.Text = "Online" Then
                    If CurrentVehicle.Name = v.FriendlyName AndAlso CurrentVehicle.Plate = v.NumberPlate Then
                        HasSecondaryJob = False
                        Online = True
                        OnlineSound.SoundPlayer(100)
                        ItemOnline.Text = "Offline"
                        MainMenu.Visible = False
                    Else
                        UI.ShowSubtitle("Please select your vehicle.")
                    End If
                Else
                    HasSecondaryJob = False
                    Online = False
                    JobAvailable = False
                    If Not CurrentPassenger = Nothing Then CurrentPassenger.Delete() : CurrentPassenger = Nothing
                    TripStarted = False
                    If Not DestBlip = Nothing Then DestBlip.Remove() : DestBlip = Nothing
                    Ticking = 500
                    PingPlayer.Stop()
                    PingPlayer.Tag = "NotPlaying"
                    Timeout = 0
                    IgnoreCount = 0
                    OfflineSound.SoundPlayer(100)
                    ItemOnline.Text = "Online"
                End If
            Case ItemMyVehicles.Text
                RefreshVehicleMenu()
            Case ItemEarnings.Text
                RefreshEarningsMenu()
        End Select
    End Sub

    Private Sub CreateEarningsMenu()
        EarningsMenu = New UIMenu("", "MY EARNINGS", New Point(0, -107))
        EarningsMenu.SetBannerType(Rectangle)
        EarningsMenu.MouseEdgeEnabled = False
        _MenuPool.Add(EarningsMenu)
        EarningsMenu.RefreshIndex()
        MainMenu.BindMenuToItem(EarningsMenu, ItemEarnings)
    End Sub

    Private Sub RefreshEarningsMenu()
        EarningsMenu.MenuItems.Clear()
        If Not CurrentProfile.Earnings.Count = 0 Then
            For Each earn In CurrentProfile.Earnings
                Dim item As New UIMenuItem($"{earn.Date} {earn.TripCount} Trip(s)", "Daily Earnings.")
                With item
                    .Tag = earn
                    '.SubString1 = earn.Date
                    '.SubString2 = earn.Earnings.ToString
                    '.SubInteger1 = earn.TripCount
                    '.SetRightLabel($"${earn.Earnings}")
                End With
                EarningsMenu.AddItem(item)
            Next
        End If
        EarningsMenu.RefreshIndex()
        MainMenu.BindMenuToItem(EarningsMenu, ItemEarnings)
    End Sub

    Private Sub EarningsMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles EarningsMenu.OnItemSelect
        Dim earn = CurrentProfile.Earnings.Find(Function(x) x.Date = CType(selectedItem.Tag, Earning).Date)
        RefreshTripsMenu(earn)
        sender.Visible = False
        TripsMenu.Visible = True
    End Sub

    Private Sub CreateVehicleMenu()
        VehicleMenu = New UIMenu("", "MY VEHICLES", New Point(0, -107))
        VehicleMenu.SetBannerType(Rectangle)
        VehicleMenu.MouseEdgeEnabled = False
        VehicleMenu.AddInstructionalButton(New InstructionalButton(GTA.Control.Jump, "Register Current Vehicle"))
        _MenuPool.Add(VehicleMenu)
        VehicleMenu.RefreshIndex()
        MainMenu.BindMenuToItem(VehicleMenu, ItemMyVehicles)
    End Sub

    Private Sub RefreshVehicleMenu()
        VehicleMenu.MenuItems.Clear()
        If Not CurrentProfile.Vehicles.Count = 0 Then
            For Each veh In CurrentProfile.Vehicles
                Dim item As New UIMenuItem($"{veh.Brand} {veh.Name} ({veh.Plate})", "Name and Plate of the Registered Vehicle.")
                With item
                    .Tag = veh
                    '.SubString1 = veh.Brand
                    '.SubString2 = veh.Name
                    '.SubString3 = veh.Plate
                    '.SubInteger1 = veh.Hash
                    '.SubInteger2 = veh.VehicleClass
                    '.SubInteger3 = veh.Color
                    '.SubInteger4 = veh.Type
                    If CurrentVehicle.NameAndPlate = .Text Then
                        .SetRightBadge(UIMenuItem.BadgeStyle.Car)
                    End If
                End With
                VehicleMenu.AddItem(item)
            Next
        End If
        VehicleMenu.RefreshIndex()
        MainMenu.BindMenuToItem(VehicleMenu, ItemMyVehicles)
    End Sub

    Private Sub VehicleMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles VehicleMenu.OnItemSelect
        Dim c As Ped = Game.Player.Character
        Dim v As Vehicle = Game.Player.Character.LastVehicle

        If c.IsInVehicle Then
            Dim uv As UberVehicle = selectedItem.Tag
            If v.FriendlyName = uv.Name AndAlso v.NumberPlate = uv.Plate Then
                CurrentVehicle = uv
                selectedItem.SetRightBadge(UIMenuItem.BadgeStyle.Car)
                ItemMyVehicles.SetRightLabel(uv.Name)
            Else
                UI.ShowSubtitle("You can't use this vehicle.")
            End If
        Else
            UI.ShowSubtitle("Get in your vehicle.")
        End If
    End Sub

    Private Sub CreateTripsMenu()
        TripsMenu = New UIMenu("", "MY TRIPS", New Point(0, -107))
        TripsMenu.SetBannerType(Rectangle)
        TripsMenu.MouseEdgeEnabled = False
        _MenuPool.Add(TripsMenu)
        TripsMenu.RefreshIndex()
    End Sub

    Private Sub RefreshTripsMenu(earn As Earning)
        TripsMenu.MenuItems.Clear()
        If Not CurrentProfile.Earnings.Count = 0 Then
            For Each trip In earn.Trips
                Dim item As New UIMenuItem($"{trip.TimeRequested} {[Enum].GetName(GetType(UberType), trip.Type)}", "Time, Type and Fare.")
                With item
                    .Tag = trip
                    '.SubString1 = trip.Date
                    '.SubString2 = trip.TimeRequested
                    '.SubString3 = [Enum].GetName(GetType(UberType), trip.Type)
                    '.SubString4 = trip.Fare.ToString
                    '.SubString5 = trip.UberFee.ToString
                    '.SubString6 = trip.YourEarnings.ToString
                    '.SubInteger1 = trip.Type
                    .SetRightLabel($"${trip.Fare}")
                End With
                TripsMenu.AddItem(item)
            Next
        End If
        TripsMenu.RefreshIndex()
        MainMenu.BindMenuToItem(TripsMenu, ItemEarnings)
    End Sub

    Private Sub Driver_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        _MenuPool.ProcessMenus()

        If VehicleMenu.Visible Then
            If Game.IsControlJustReleased(0, GTA.Control.Jump) Then
                If Game.Player.Character.IsInVehicle() Then
                    Dim v As Vehicle = Game.Player.Character.CurrentVehicle
                    CurrentProfile.Vehicles.Add(New UberVehicle(v.FriendlyName, v.Brand, v.NumberPlate, v.Model.Hash, v.PrimaryColor, v.ClassType, v.UberType))
                    CurrentProfile.ProfileFileName = Path.GetFullPath($".\scripts\UberDriver\UserProfiles\{Game.Player.Character.Name}.xml")
                    CurrentProfile.Save()
                    RefreshVehicleMenu()
                End If
            End If
        End If

        If Online Then
            If Ticking >= 0 Then Ticking -= 1

            If Ticking <= 0 AndAlso CurrentPassenger = Nothing Then
                CurrentPassenger = GetRider()
                JobAvailable = True
            End If

            If JobAvailable Then
                If Not PingPlayer.Tag = "IsPlaying" Then
                    PingPlayer = PingSound.SoundPlayer(100, True)
                    Dim mile As Single = World.CalculateTravelDistance(Game.Player.Character.LastVehicle.Position, CurrentPassenger.Position).MeterToMiles
                    Dim min As Integer = System.Math.Round(World.CalculateTravelDistance(Game.Player.Character.LastVehicle.Position, CurrentPassenger.Position).MeterToKilometers)
                    Dim rd As New Random
                    DisplayNotificationThisFrame("UBER", "", $"{min} min~n~{mile} mi   {[Enum].GetName(GetType(UberType), Game.Player.Character.LastVehicle.UberType)}   {rd.Next(0, 500) / 100}", Icon.CHAR_MULTIPLAYER, True, IconType.DollarSignIcon)
                End If
                DrawInstructionalButton()
                Timeout += 1
                If Timeout >= 900 Then
                    JobAvailable = False
                    Ticking = 500
                    PingPlayer.Stop()
                    PingPlayer.Tag = "NotPlaying"
                    Timeout = 0
                    IgnoreCount += 1
                End If
                If IgnoreCount >= 3 Then
                    HasSecondaryJob = False
                    Online = False
                    JobAvailable = False
                    Ticking = 500
                    PingPlayer.Stop()
                    PingPlayer.Tag = "NotPlaying"
                    Timeout = 0
                    IgnoreCount = 0
                    OfflineSound.SoundPlayer(100)
                    ItemOnline.Text = "Online"
                    DisplayNotificationThisFrame("UBER", "", "Are you still there?", Icon.CHAR_MULTIPLAYER, True, IconType.DollarSignIcon)
                End If
                If Game.IsControlJustReleased(0, GTA.Control.FrontendAccept) Then
                    PingPlayer.Stop()
                    PingPlayer.Tag = "NotPlaying"
                    JobAvailable = False
                    CurrentPassenger.CurrentBlip.ShowRoute = True
                    Timeout = 0
                End If
                If Game.IsControlJustReleased(0, GTA.Control.PhoneCancel) Then
                    JobAvailable = False
                    Ticking = 500
                    PingPlayer.Stop()
                    PingPlayer.Tag = "NotPlaying"
                    Timeout = 0
                End If
            End If

            If Not CurrentPassenger = Nothing Then
                If Game.Player.Character.LastVehicle.Position.DistanceTo(CurrentPassenger.Position) <= 5.0F Then
                    If Not CurrentPassenger.IsInVehicle(Game.Player.Character.LastVehicle) Then
                        CurrentPassenger.Task.EnterVehicle(Game.Player.Character.LastVehicle, VehicleSeat.Passenger, 10000, 1.0)
                        CurrentPassenger.AlwaysKeepTask = True
                        Wait(1000)
                    Else
                        CurrentPassenger.Task.ChatTo(Game.Player.Character)
                        CurrentPassenger.AlwaysKeepTask = True
                        If Not TripStarted Then
                            DrawInstructionalButtonStartTrip()
                        End If
                        If Game.IsControlJustReleased(0, GTA.Control.FrontendAccept) Then
                            CurrentPassenger.CurrentBlip.Remove()
                            CurrentDestination = Vector3.RandomXYZ
                            DestBlip = World.CreateBlip(CurrentDestination)
                            DestBlip.Color = BlipColor.Yellow
                            DestBlip.Name = World.GetStreetName(CurrentDestination)
                            DestBlip.ShowRoute = True
                            PickupLocation = Game.Player.Character.Position
                            DropoffLocation = CurrentDestination
                            TripStarted = True
                        End If
                        If Game.IsControlJustReleased(0, GTA.Control.PhoneCancel) Then
                            JobAvailable = False
                            CurrentPassenger.CurrentBlip.Remove()
                            CurrentPassenger.LeaveGroup()
                            CurrentPassenger.IsPersistent = False
                            CurrentPassenger.MarkAsNoLongerNeeded()
                            CurrentPassenger = Nothing
                        End If
                    End If
                End If

                If Game.Player.Character.LastVehicle.Position.DistanceTo(CurrentDestination) <= 5.0F Then
                    If Game.Player.Character.LastVehicle.IsStopped Then
                        Dim earningf As Single = World.CalculateTravelDistance(PickupLocation, DropoffLocation).MeterToKilometers
                        Select Case Game.Player.Character.LastVehicle.UberType
                            Case UberType.UberBLACK
                                earningf = earningf * 4.0F
                            Case UberType.UberSelect
                                earningf = earningf * 2.0F
                            Case UberType.UberXL
                                earningf = earningf * 1.3F
                        End Select

                        Dim earningi As Integer = System.Math.Round(earningf)
                        Dim tips As Single = (earningi - earningf)
                        Game.Player.Character.Money = (Game.Player.Character.Money + earningi)
                        UI.ShowSubtitle($"You Earned: ${earningf} + ${tips} Total: ${earningi}")
                        Dim earning As Earning = CurrentProfile.Earnings.Find(Function(x) x.Date = World.CurrentDate.ToShortDateString)
                        If earning.Date = Nothing Then
                            earning = New Earning(World.CurrentDate.ToShortDateString, New List(Of Trip))
                        End If
                        Dim trip As New Trip(World.CurrentDate.ToShortTimeString, World.CurrentDate.ToShortDateString, Game.Player.Character.LastVehicle.UberType, earningf, earningf * 0.2)
                        earning.Trips.Add(trip)
                        CurrentProfile.Earnings.Add(earning)
                        CurrentProfile.ProfileFileName = Path.GetFullPath($".\scripts\UberDriver\UserProfiles\{Game.Player.Character.Name}.xml")
                        CurrentProfile.Save()

                        DestBlip.Remove()
                        DestBlip = Nothing
                        CurrentPassenger.Task.LeaveVehicle(Game.Player.Character.LastVehicle, True)
                        CurrentPassenger.LeaveGroup()
                        CurrentPassenger.IsPersistent = False
                        CurrentPassenger.MarkAsNoLongerNeeded()
                        CurrentPassenger = Nothing
                        TripStarted = False
                        JobAvailable = False
                    End If
                End If
            End If
        End If

        If "uber".Cheating Then
            RefreshMainMenu()
            MainMenu.Visible = True
        End If

        If "savepos".Cheating Then
            Dim vector As Vector3 = Game.Player.Character.LastVehicle.Position
            Dim name = Game.GetUserInput(World.GetStreetName(vector), 9999)
            Dim pop = Game.GetUserInput("0.5", 32)
            Dim place As New Place(name, vector, CSng(pop))
            Places.Add(place)
        End If

        If "saveplaces".Cheating Then
            'Locations.FileName = POIxml
            'Locations.Places = Places
            'Locations.Save()
            Dim newloc As New LocationData(POIxml) With {.Places = Places}
            newloc.Save()
        End If
    End Sub

    Private Sub Driver_Aborted(sender As Object, e As EventArgs) Handles Me.Aborted
        If Not DestBlip = Nothing Then DestBlip.Remove()
        If CurrentPassenger.CurrentBlip.Exists Then CurrentPassenger.CurrentBlip.Remove()
        If Not CurrentPassenger = Nothing Then CurrentPassenger.Delete()
    End Sub

    'Private Sub Driver_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
    '    If e.KeyCode = Keys.O Then
    '        Dim pos As Vector3 = Game.Player.Character.Position
    '        'PointOfInterest.Add(New Vector3(0, 0, 0))
    '        logger.Log($"PointOfInterest.Add(New Vector3({pos.X}, {pos.Y}, {pos.Z})) '{World.GetStreetName(pos)}")
    '        UI.ShowSubtitle($"{World.GetStreetName(pos)} Added.")
    '    End If
    'End Sub
End Class
