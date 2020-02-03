Imports System.IO
Imports System.Media
Imports System.Runtime.CompilerServices
Imports GTA
Imports GTA.Math
Imports GTA.Native

Module Helper

    Public PointOfInterest As New List(Of Vector3)
    Public POIxml As String = Path.GetFullPath($".\scripts\UberDriver\Places.xml")
    Public PingSound As String = "scripts\UberDriver\Sounds\ping_new.wav"
    Public OnlineSound As String = "scripts\UberDriver\Sounds\online_new.wav"
    Public OfflineSound As String = "scripts\UberDriver\Sounds\offline_new.wav"
    Public CancelSound As String = "scripts\UberDriver\Sounds\cancel_new.wav"
    Public Locations As LocationClass
    Public CurrentProfile As UberProfileData
    Public Places As New List(Of Place)

    Public Function GET_CONTROL_INSTRUCTIONAL_BUTTON(control As GTA.Control) As String
        Return Native.Function.Call(Of String)(Hash._0x0499D7B09FC9B407, 2, control, 0)
    End Function

    <Extension()>
    Public Sub DRAW_SCALEFORM_MOVIE_FULLSCREEN(sc As Scaleform)
        Native.Function.Call(Hash.DRAW_SCALEFORM_MOVIE_FULLSCREEN, sc.Handle, 255, 255, 255, 255, 0)
    End Sub

    <Extension()>
    Public Sub Disable(control As GTA.Control)
        Native.Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, control)
    End Sub

    <Extension()>
    Public Function Cheating(Cheat As String) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash._0x557E43C447E700A8, Game.GenerateHash(Cheat))
    End Function

    <Extension()>
    Public Function SoundPlayer(waveFile As String, volume As Integer, loopAudio As Boolean) As SoundPlayer
        Dim player As SoundPlayer
        Using stream As New WaveStream(IO.File.OpenRead(waveFile))
            stream.Volume = volume
            Using ply As New SoundPlayer(stream)
                If loopAudio Then ply.PlayLooping() : ply.Tag = "IsPlaying" Else ply.Play()
                player = ply
            End Using
        End Using
        Return player
    End Function

    <Extension()>
    Public Sub SoundPlayer(waveFile As String, volume As Integer)
        Using stream As New WaveStream(IO.File.OpenRead(waveFile))
            stream.Volume = volume
            Using player As New SoundPlayer(stream)
                player.Play()
            End Using
        End Using
    End Sub

    Public Sub DisplayHelpTextThisFrame(ByVal [text] As String)
        Dim arguments As InputArgument() = New InputArgument() {"STRING"}
        Native.Function.Call(Hash._0x8509B634FBE7DA11, arguments)
        Dim argumentArray2 As InputArgument() = New InputArgument() {[text]}
        Native.Function.Call(Hash._0x6C188BE134E074AA, argumentArray2)
        Dim argumentArray3 As InputArgument() = New InputArgument() {0, 0, 1, -1}
        Native.Function.Call(Hash._0x238FFE5C7B0498A6, argumentArray3)
    End Sub

    Public Sub DisplayNotificationThisFrame(Sender As String, Subject As String, Message As String, Icon As Icon, Flash As Boolean, Type As IconType)
        Native.Function.Call(Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING")
        Native.Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, Message)
        Native.Function.Call(Hash._SET_NOTIFICATION_MESSAGE, Icon.ToString("F"), Icon.ToString("F"), Flash, Type, Sender, Subject)
        Native.Function.Call(Hash._DRAW_NOTIFICATION, False, True)
    End Sub

    Public Enum IconType
        ChatBox = 1
        Email = 2
        AddFriendRequest = 3
        Nothing4 = 4
        Nothing5 = 5
        Nothing6 = 6
        RightJumpingArrow = 7
        RPIcon = 8
        DollarSignIcon = 9
    End Enum

    Public Enum Icon
        CHAR_ABIGAIL
        CHAR_ALL_PLAYERS_CONF
        CHAR_AMANDA
        CHAR_AMMUNATION
        CHAR_ANDREAS
        CHAR_ANTONIA
        CHAR_ARTHUR
        CHAR_ASHLEY
        CHAR_BANK_BOL
        CHAR_BANK_FLEECA
        CHAR_BANK_MAZE
        CHAR_BARRY
        CHAR_BEVERLY
        CHAR_BIKESITE
        CHAR_BLANK_ENTRY
        CHAR_BLIMP
        CHAR_BLOCKED
        CHAR_BOATSITE
        CHAR_BROKEN_DOWN_GIRL
        CHAR_BUGSTARS
        CHAR_CALL911
        CHAR_CARSITE
        CHAR_CARSITE2
        CHAR_CASTRO
        CHAR_CHAT_CALL
        CHAR_CHEF
        CHAR_CHENG
        CHAR_CHENGSR
        CHAR_CHOP
        CHAR_CREATOR_PORTRAITS
        CHAR_CRIS
        CHAR_DAVE
        CHAR_DEFAULT
        CHAR_DENISE
        CHAR_DETONATEBOMB
        CHAR_DETONATEPHONE
        CHAR_DEVIN
        CHAR_DIAL_A_SUB
        CHAR_DOM
        CHAR_DOMESTIC_GIRL
        CHAR_DREYFUSS
        CHAR_DR_FRIEDLANDER
        CHAR_EPSILON
        CHAR_ESTATE_AGENT
        CHAR_FACEBOOK
        CHAR_FILMNOIR
        CHAR_FLOYD
        CHAR_FRANKLIN
        CHAR_FRANK_TREV_CONF
        CHAR_GAYMILITARY
        CHAR_HAO
        CHAR_HITCHER_GIRL
        CHAR_HUMANDEFAULT
        CHAR_HUNTER
        CHAR_JIMMY
        CHAR_JIMMY_BOSTON
        CHAR_JOE
        CHAR_JOSEF
        CHAR_JOSH
        CHAR_LAMAR
        CHAR_LAZLOW
        CHAR_LESTER
        CHAR_LESTER_DEATHWISH
        CHAR_LEST_FRANK_CONF
        CHAR_LEST_MIKE_CONF
        CHAR_LIFEINVADER
        CHAR_LS_CUSTOMS
        CHAR_LS_TOURIST_BOARD
        CHAR_MANUEL
        CHAR_MARNIE
        CHAR_MARTIN
        CHAR_MARY_ANN
        CHAR_MAUDE
        CHAR_MECHANIC
        CHAR_MICHAEL
        CHAR_MIKE_FRANK_CONF
        CHAR_MIKE_TREV_CONF
        CHAR_MILSITE
        CHAR_MINOTAUR
        CHAR_MOLLY
        CHAR_MP_ARMY_CONTACT
        CHAR_MP_BIKER_BOSS
        CHAR_MP_BIKER_MECHANIC
        CHAR_MP_BRUCIE
        CHAR_MP_DETONATEPHONE
        CHAR_MP_FAM_BOSS
        CHAR_MP_FIB_CONTACT
        CHAR_MP_FM_CONTACT
        CHAR_MP_GERALD
        CHAR_MP_JULIO
        CHAR_MP_MECHANIC
        CHAR_MP_MERRYWEATHER
        CHAR_MP_MEX_BOSS
        CHAR_MP_MEX_DOCKS
        CHAR_MP_MEX_LT
        CHAR_MP_MORS_MUTUAL
        CHAR_MP_PROF_BOSS
        CHAR_MP_RAY_LAVOY
        CHAR_MP_ROBERTO
        CHAR_MP_SNITCH
        CHAR_MP_STRETCH
        CHAR_MP_STRIPCLUB_PR
        CHAR_MRS_THORNHILL
        CHAR_MULTIPLAYER
        CHAR_NIGEL
        CHAR_OMEGA
        CHAR_ONEIL
        CHAR_ORTEGA
        CHAR_OSCAR
        CHAR_PATRICIA
        CHAR_PEGASUS_DELIVERY
        CHAR_PLANESITE
        CHAR_PROPERTY_ARMS_TRAFFICKING
        CHAR_PROPERTY_BAR_AIRPORT
        CHAR_PROPERTY_BAR_BAYVIEW
        CHAR_PROPERTY_BAR_CAFE_ROJO
        CHAR_PROPERTY_BAR_COCKOTOOS
        CHAR_PROPERTY_BAR_ECLIPSE
        CHAR_PROPERTY_BAR_FES
        CHAR_PROPERTY_BAR_HEN_HOUSE
        CHAR_PROPERTY_BAR_HI_MEN
        CHAR_PROPERTY_BAR_HOOKIES
        CHAR_PROPERTY_BAR_IRISH
        CHAR_PROPERTY_BAR_LES_BIANCO
        CHAR_PROPERTY_BAR_MIRROR_PARK
        CHAR_PROPERTY_BAR_PITCHERS
        CHAR_PROPERTY_BAR_SINGLETONS
        CHAR_PROPERTY_BAR_TEQUILALA
        CHAR_PROPERTY_BAR_UNBRANDED
        CHAR_PROPERTY_CAR_MOD_SHOP
        CHAR_PROPERTY_CAR_SCRAP_YARD
        CHAR_PROPERTY_CINEMA_DOWNTOWN
        CHAR_PROPERTY_CINEMA_MORNINGWOOD
        CHAR_PROPERTY_CINEMA_VINEWOOD
        CHAR_PROPERTY_GOLF_CLUB
        CHAR_PROPERTY_PLANE_SCRAP_YARD
        CHAR_PROPERTY_SONAR_COLLECTIONS
        CHAR_PROPERTY_TAXI_LOT
        CHAR_PROPERTY_TOWING_IMPOUND
        CHAR_PROPERTY_WEED_SHOP
        CHAR_RON
        CHAR_SAEEDA
        CHAR_SASQUATCH
        CHAR_SIMEON
        CHAR_SOCIAL_CLUB
        CHAR_SOLOMON
        CHAR_STEVE
        CHAR_STEVE_MIKE_CONF
        CHAR_STEVE_TREV_CONF
        CHAR_STRETCH
        CHAR_STRIPPER_CHASTITY
        CHAR_STRIPPER_CHEETAH
        CHAR_STRIPPER_FUFU
        CHAR_STRIPPER_INFERNUS
        CHAR_STRIPPER_JULIET
        CHAR_STRIPPER_NIKKI
        CHAR_STRIPPER_PEACH
        CHAR_STRIPPER_SAPPHIRE
        CHAR_TANISHA
        CHAR_TAXI
        CHAR_TAXI_LIZ
        CHAR_TENNIS_COACH
        CHAR_TOW_TONYA
        CHAR_TRACEY
        CHAR_TREVOR
        CHAR_WADE
        CHAR_YOUTUBE
    End Enum

    Public UberX As New List(Of Model) From {VehicleHash.Asea, VehicleHash.Dilettante, VehicleHash.Moonbeam, VehicleHash.Moonbeam2, VehicleHash.Freecrawler, VehicleHash.Kamacho, VehicleHash.Mesa3,
        VehicleHash.RancherXL, VehicleHash.Sandking, VehicleHash.Asterope, VehicleHash.Fugitive, VehicleHash.Glendale, VehicleHash.Ingot, VehicleHash.Intruder, VehicleHash.Premier, VehicleHash.Primo,
        VehicleHash.Primo2, VehicleHash.Stanier, VehicleHash.Stratum, VehicleHash.Surge, VehicleHash.Washington, VehicleHash.Mesa, VehicleHash.Landstalker, VehicleHash.Habanero, VehicleHash.Gresley,
        VehicleHash.FQ2, VehicleHash.Cavalcade, VehicleHash.BJXL, VehicleHash.Sultan, VehicleHash.Baller, VehicleHash.Patriot, VehicleHash.Radi, VehicleHash.Seminole, VehicleHash.Sadler, VehicleHash.Bison,
        VehicleHash.Minivan, VehicleHash.Minivan2, VehicleHash.Rumpo3}
    Public UberSelect As New List(Of Model) From {VehicleHash.Exemplar, VehicleHash.Felon, VehicleHash.Jackal, VehicleHash.Oracle, VehicleHash.Oracle2, VehicleHash.Schafter2, VehicleHash.Tailgater,
        VehicleHash.Buffalo, VehicleHash.Buffalo2, VehicleHash.Buffalo3, VehicleHash.Kuruma, VehicleHash.Neon, VehicleHash.Raiden, VehicleHash.Schafter3, VehicleHash.Schafter4, VehicleHash.Contender,
        VehicleHash.Streiter, VehicleHash.Baller2, VehicleHash.Baller3, VehicleHash.Baller4, VehicleHash.Cavalcade2, VehicleHash.Rocoto, VehicleHash.Serrano}
    Public UberBLACK As New List(Of Model) From {VehicleHash.Windsor2, VehicleHash.Cog55, VehicleHash.Cognoscenti, VehicleHash.Stafford, VehicleHash.Stretch, VehicleHash.Superd, VehicleHash.Dubsta,
        VehicleHash.Dubsta2, VehicleHash.BType3, VehicleHash.BType, VehicleHash.BType2, VehicleHash.Revolter, VehicleHash.Huntley, -420911112, VehicleHash.XLS}
    Public UberXL As New List(Of Model) From {VehicleHash.Guardian, VehicleHash.Dubsta3, VehicleHash.Granger}

    <Extension>
    Public Function UberType(vehicle As Vehicle) As UberType
        If UberX.Contains(vehicle.Model) Then Return UberType.UberX
        If UberSelect.Contains(vehicle.Model) Then Return UberType.UberSelect
        If UberBLACK.Contains(vehicle.Model) Then Return UberType.UberBLACK
        If UberXL.Contains(vehicle.Model) Then Return UberType.UberXL
        Return UberType.NotSuitable
    End Function

    <Extension()>
    Public Function HasRearDoors(vehicle As Vehicle) As Boolean
        If vehicle.HasBone("door_dside_r") AndAlso vehicle.HasBone("door_pside_r") Then Return True Else Return False
    End Function

    <Extension()>
    Public Function HasTurret(vehicle As Vehicle) As Boolean
        Return vehicle.HasBone("turret_1base")
    End Function

    <Extension()>
    Public Function Name(ped As Ped) As String
        Select Case ped.Model
            Case PedHash.Franklin
                Return "Franklin"
            Case PedHash.Michael
                Return "Michael"
            Case PedHash.Trevor
                Return "Trevor"
            Case Else
                Return "Player"
        End Select
    End Function

    <Extension()>
    Public Function NameAndPlate(vehicle As UberVehicle) As String
        On Error Resume Next
        Return $"{vehicle.Brand} {vehicle.Name} ({vehicle.Plate})"
    End Function

    Public Function GetRider() As Ped
        Dim rd As New Random
        Dim newPos As Vector3 = World.GetNextPositionOnSidewalk(Vector3.RandomXYZ)
        Dim ped As Ped = World.CreateRandomPed(newPos)
        ped.IsPersistent = True
        Game.Player.Character.CurrentPedGroup.Add(ped, True)
        ped.NeverLeavesGroup = True
        ped.RelationshipGroup = Game.Player.Character.RelationshipGroup
        ped.Task.Wait(300000) '5 minutes
        ped.AlwaysKeepTask = True
        Dim pedblip As Blip = ped.AddBlip
        With pedblip
            .Sprite = BlipSprite.Friend
            .Color = BlipColor.Blue
            .IsFriendly = True
            .Name = "Passenger"
        End With
        Return ped
    End Function

    <Extension()>
    Public Function MeterToMiles(meter As Single) As Single
        Return meter * 0.000621371
    End Function

    <Extension()>
    Public Function MeterToKilometers(meter As Single) As Single
        Return meter * 0.001
    End Function

    Public Sub AddPointOfInterest()
        PointOfInterest.Add(New Vector3(0, 0, 0))
    End Sub

End Module
