Imports System.Windows.Threading
Imports System.IO

Class MainWindow

    ' Timers
    Dim moveTimer As DispatcherTimer
    Dim speakTimer As DispatcherTimer

    ' Secret button / videos
    Dim secretClickCount As Integer = 0
    Dim booVideos As New List(Of String)

    ' Movement directions
    Dim direction As Integer = 1 ' For phone lining
    Private speakSprite As Integer = 1
    Private speakMinLeft As Double
    Private speakMaxLeft As Double

    ' Phone state flags
    Private phonePickedUp As Boolean = False

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        ' Background music
        mediaBGMusic.Source = New Uri(System.IO.Path.GetFullPath("pickytures/bgmusic.mp3"))
        mediaBGMusic.Volume = 0.5
        mediaBGMusic.Play()

        ' Ring sound
        mediaRing.Source = New Uri(System.IO.Path.GetFullPath("pickytures/phoneringaudio.mp3"))
        mediaRing.Volume = 0

        ' Load boo videos
        For i = 1 To 7
            booVideos.Add(System.IO.Path.GetFullPath("pickytures/boo" & i & ".mp4"))
        Next

        ' Timers
        moveTimer = New DispatcherTimer()
        moveTimer.Interval = TimeSpan.FromMilliseconds(120)
        AddHandler moveTimer.Tick, AddressOf MovePhone

        speakTimer = New DispatcherTimer()
        speakTimer.Interval = TimeSpan.FromMilliseconds(120)
        AddHandler speakTimer.Tick, AddressOf AnimateSpeaking

        ' Event handlers
        AddHandler imgBlackOpen.MouseDown, AddressOf StartGame
        AddHandler imgPhoneDown.MouseDown, AddressOf PickUpPhone
        AddHandler btnSecret.Click, AddressOf SecretClicked

        ' Hide video initially
        mediaVideo.Visibility = Visibility.Hidden
        AddHandler mediaVideo.MediaEnded, Sub(s, ev)
                                              mediaVideo.Visibility = Visibility.Hidden
                                          End Sub
    End Sub

    ' -------------------------
    ' Start Game - fade black
    ' -------------------------
    Private Async Sub StartGame(sender As Object, e As MouseButtonEventArgs)
        For i = 1 To 100
            imgBlackOpen.Opacity -= 0.01
            Await Task.Delay(10)
        Next

        imgBlackOpen.Visibility = Visibility.Hidden
        Await Task.Delay(2000)

        imgPhoneDown.Visibility = Visibility.Visible
        imgPhoneLining.Visibility = Visibility.Visible

        moveTimer.Start()

        ' Start phone ringing sequence
        phonePickedUp = False
        StartPhoneRingSequence()
    End Sub

    ' -------------------------
    ' Phone ring sequence with delays
    ' -------------------------
    Private Async Sub StartPhoneRingSequence()
        ' Wait 2 seconds
        Await Task.Delay(2000)
        If phonePickedUp Then Return

        ' Start silent ringing
        mediaRing.Volume = 0
        mediaRing.Play()

        ' Wait 4.5 seconds
        Await Task.Delay(4500)
        If phonePickedUp Then Return

        ' Gradually increase volume
        For i = 1 To 100
            mediaRing.Volume += 0.01
            Await Task.Delay(30)
            If phonePickedUp Then
                mediaRing.Stop()
                Return
            End If
        Next

        ' Wait 20 seconds before auto-reset
        Dim elapsed As Integer = 0
        While elapsed < 20000
            Await Task.Delay(1000)
            elapsed += 1000
            If phonePickedUp Then
                mediaRing.Stop()
                Return
            End If
        End While

        ' Restart game
        ResetGame()
    End Sub

    ' -------------------------
    ' Phone lining animation
    ' -------------------------
    Private Sub MovePhone(sender As Object, e As EventArgs)
        Dim currentTop As Double = Canvas.GetTop(imgPhoneLining)

        If direction = 1 Then
            currentTop += 5
        Else
            currentTop -= 5
        End If

        If currentTop > 180 Then direction = -1
        If currentTop < 120 Then direction = 1

        Canvas.SetTop(imgPhoneLining, currentTop)
    End Sub

    ' -------------------------
    ' Pick up phone
    ' -------------------------
    Private Sub PickUpPhone(sender As Object, e As MouseButtonEventArgs)
        phonePickedUp = True ' stop timers/sequences

        moveTimer.Stop()
        mediaRing.Stop()

        ' Hide phone + lining
        imgPhoneDown.Visibility = Visibility.Hidden
        imgPhoneLining.Visibility = Visibility.Hidden

        ' Show picked up background + speaking
        imgShadowHand.Visibility = Visibility.Visible
        imgPickedUp.Visibility = Visibility.Visible
        imgSpeaking.Visibility = Visibility.Visible

        ' Set shake boundaries
        Dim startLeft = Canvas.GetLeft(imgSpeaking)
        speakMinLeft = startLeft - 10
        speakMaxLeft = startLeft + 10
        speakTimer.Start()
    End Sub

    ' -------------------------
    ' Animate Speaking Sprite
    ' -------------------------
    Private Sub AnimateSpeaking(sender As Object, e As EventArgs)
        Dim currentLeft As Double = Canvas.GetLeft(imgSpeaking)
        currentLeft += 2 * speakSprite

        If currentLeft > speakMaxLeft Then speakSprite = -1
        If currentLeft < speakMinLeft Then speakSprite = 1

        Canvas.SetLeft(imgSpeaking, currentLeft)
    End Sub

    ' -------------------------
    ' Reset Game
    ' -------------------------
    Private Sub ResetGame()
        speakTimer.Stop()
        phonePickedUp = False

        imgPhoneDown.Visibility = Visibility.Hidden
        imgPhoneLining.Visibility = Visibility.Hidden
        imgPickedUp.Visibility = Visibility.Hidden
        imgSpeaking.Visibility = Visibility.Hidden
        imgShadowHand.Visibility = Visibility.Hidden

        imgBlackOpen.Opacity = 1
        imgBlackOpen.Visibility = Visibility.Visible
    End Sub

    ' -------------------------
    ' Secret button click - play random video
    ' -------------------------
    Private Sub SecretClicked(sender As Object, e As RoutedEventArgs)
        secretClickCount += 1

        If secretClickCount = 3 Then
            secretClickCount = 0
            Dim rnd As New Random()
            Dim index As Integer = rnd.Next(0, booVideos.Count)
            mediaVideo.Source = New Uri(booVideos(index))
            mediaVideo.Visibility = Visibility.Visible
            mediaVideo.Play()
        End If
    End Sub
End Class