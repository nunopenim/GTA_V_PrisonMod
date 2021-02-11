Imports System
Imports System.Windows.forms
Imports System.Drawing
Imports System.IO
Imports GTA
Imports GTA.Math
Imports GTA.native
Public Class Prison
    Inherits Script
    Public Sub New()
        Me.Interval = 10
    End Sub
    Private arrested As Boolean = False, inprison As Boolean = False, riot As Boolean = False
    Private tryingesc As Byte = 0
    Private Rndclothes As New Random, clothes As Integer
    'Private carcel As New Vector3(1654.32, 2522.413, 47)
    Private carcel As New Vector3(1691.334, 2545.26, 46)
    Private guards As Ped()
    Shadows Sub keydown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.F11 Then
            Game.Player.Character.Position = carcel
            inprison = True
            arrested = False
            clotheselect()
        End If
		If e.KeyCode = Keys.F10 Then
			Game.Player.Character.Weapons.Give(WeaponHash.FlareGun, 10, true, true)
		End If
        If e.KeyCode = Keys.F8 Then
			Game.Player.Character.Weapons.Give(WeaponHash.Pistol, 9999, true, true)
		End If
        'F9 taken by vehicle spawner
    End Sub

    Private Sub general_tick(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Tick
        If Game.Player.WantedLevel > 2 Or inprison = True Then     'If player dies with 3 or more stars, right to jail!
            If Game.Player.Character.IsDead = True Then
                arrested = True
                Game.Player.Character.Weapons.RemoveAll()
                tryingesc = 0
            End If
        End If

        If arrested = True And Game.Player.CanControlCharacter Then    
            Game.Player.Character.Position = carcel                     
            inprison = True   
            arrested = False
            clotheselect()
        End If

        If inprison = True Then    
            enprision()
        End If

        'UI.ShowSubtitle("Tyingesc: " & tryingesc.ToString & " Arrested: " & arrested.ToString & " inprison: " & inprison.ToString)
    End Sub

    Private Sub enprision()        
        guards = World.GetAllPeds("s_m_m_prisguard_01")     
        For Each guard As Ped In guards
            If guard.Weapons.Current.Hash = WeaponHash.AssaultRifle Then  
                guard.Delete()
            ElseIf guard.Weapons.Current.Hash = WeaponHash.SniperRifle Then     'Lowering the accuracy, so we actually can escape the prison
                guard.Accuracy = 0.1
            ElseIf guard.Money <> 5 Then
                guard.Weapons.RemoveAll()
                guard.Weapons.Give(WeaponHash.Nightstick, 1, True, True)     
                guard.Task.WanderAround()
                guard.Money = 5
            End If
            If guard.HasBeenDamagedBy(Game.Player.Character) = True Then
                tryingesc = 2
            End If
            If tryingesc > 0 Then
                guard.AlwaysKeepTask = True
                guard.Task.FightAgainst(Game.Player.Character)
            End If
        Next

        If tryingesc = 0 Then     
            GTA.Native.Function.Call(Hash.SET_MAX_WANTED_LEVEL, 0)
            GTA.Native.Function.Call(Hash.SET_PLAYER_WANTED_LEVEL, Game.Player, 0, False)
            GTA.Native.Function.Call(Hash.SET_PLAYER_WANTED_LEVEL_NOW, False)
            GTA.Native.Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player, True)
            GTA.Native.Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, Game.Player, True)
            GTA.Native.Function.Call(Hash.STOP_ALARM, "PRISON_ALARMS", True)
        Else
            If tryingesc = 1 Then       
                GTA.Native.Function.Call(Hash.SET_MAX_WANTED_LEVEL, 1)      
                GTA.Native.Function.Call(Hash.SET_PLAYER_WANTED_LEVEL, Game.Player, 1, False)
                GTA.Native.Function.Call(Hash.SET_PLAYER_WANTED_LEVEL_NOW, False)
            Else        'tryingesc = 2 means player has used a firearm on a guard
                GTA.Native.Function.Call(Hash.SET_MAX_WANTED_LEVEL, 2)    
            End If
            GTA.Native.Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player, False)
            GTA.Native.Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, Game.Player, False)
            GTA.Native.Function.Call(Hash.STOP_ALARM, "PRISON_ALARMS", False)
        End If

        If tryingesc = 0 Then 
            If Game.Player.Character.Position.Z > 52 Or Game.Player.Character.Position.DistanceTo(carcel) > 100 Then  'If player in high place or far from jail
                tryingesc = 1 
            End If
        End If

        If Game.Player.Character.IsShooting = True Then     
            tryingesc = 2     
        End If

        If Game.Player.Character.Position.DistanceTo(carcel) > 170 Then
            tryingesc = 0
            GTA.Native.Function.Call(Hash.SET_MAX_WANTED_LEVEL, 5)    
            GTA.Native.Function.Call(Hash.SET_PLAYER_WANTED_LEVEL, Game.Player, 3, False)
            GTA.Native.Function.Call(Hash.SET_PLAYER_WANTED_LEVEL_NOW, False)
            GTA.Native.Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player, False)
            GTA.Native.Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, Game.Player, False)
            'escaped = True     
            inprison = False    
        End If
    End Sub

    Private Sub clotheselect()
        'Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 8, 0, 0, 0)   
        Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 9, 0, 0, 0)   
        Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 10, 0, 0, 0)   
        Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 11, 0, 0, 0)   

        If Game.Player.Character.Model.Hash = -1686040670 Then  'Player_two=Trevor
            clothes = Rndclothes.Next(2)
            Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 8, 14, 0, 0)  
            Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 4, 5, 2, 1) 
            Select Case clothes
                Case 0
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 16, 0, 1)  
                Case Else
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 5, 2, 1)   'Jail dressing
            End Select
        ElseIf Game.Player.Character.Model.Hash = -1692214353 Then    'Player_one=Franklin
            clothes = Rndclothes.Next(5)
            Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 8, 14, 0, 0)  
            Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 4, 1, 5, 1)  
            Select Case clothes
                Case 0
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 0, 0, 1)   
                Case 1
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 15, 0, 1) 
                Case 2
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 11, 0, 1)  
                Case 3
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 26, 0, 1) 
                Case Else
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 1, 5, 1)   
            End Select
        ElseIf Game.Player.Character.Model.Hash = 225514697 Then    'Player_zero=Michael
            clothes = Rndclothes.Next(4)
            Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 8, 0, 0, 0)   
            Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 4, 11, 4, 1)  
            Select Case clothes
                Case 0
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 19, 0, 1)  
                Case 1
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 2, 10, 0)  
                Case 2
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 26, 0, 1)  
                Case Else
                    Native.Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character, 3, 12, 4, 1) 
            End Select
        End If
    End Sub

    'Private Sub iniciar()
    '    inprison = False
    '    arrested = False
    'End Sub
End Class
