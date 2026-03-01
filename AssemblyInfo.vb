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

        imgBlackOpen_Copy.Visibility = Visibility.Hidden

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

        ' Boo video auto-hide when finished
        AddHandler mediaBooVideo.MediaEnded, Sub()
                                                 mediaBooVideo.Visibility = Visibility.Hidden
                                                 mediaBooVideo.Stop()
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
        phonePickedUp = False
        StartPhoneRingSequence()
    End Sub

    ' -------------------------
    ' Phone ring sequence
    ' -------------------------
    Private Async Sub StartPhoneRingSequence()
        Await Task.Delay(2000)
        If phonePickedUp Then Return

        mediaRing.Volume = 0
        mediaRing.Play()

        Await Task.Delay(4500)
        If phonePickedUp Then Return

        For i = 1 To 100
            mediaRing.Volume += 0.01
            Await Task.Delay(30)
            If phonePickedUp Then
                mediaRing.Stop()
                Return
            End If
        Next

        Dim elapsed As Integer = 0
        While elapsed < 20000
            Await Task.Delay(1000)
            elapsed += 1000
            If phonePickedUp Then
                mediaRing.Stop()
                Return
            End If
        End While

        ResetGame()
    End Sub

    ' -------------------------
    ' Phone lining animation
    ' -------------------------
    Private Sub MovePhone(sender As Object, e As EventArgs)
        Dim currentTop As Double = Canvas.GetTop(imgPhoneLining)

        If direction = 1 Then currentTop += 5 Else currentTop -= 5
        If currentTop > 180 Then direction = -1
        If currentTop < 120 Then direction = 1

        Canvas.SetTop(imgPhoneLining, currentTop)
    End Sub

    ' -------------------------
    ' Pick up phone
    ' -------------------------
    Private Sub PickUpPhone(sender As Object, e As MouseButtonEventArgs)
        phonePickedUp = True
        moveTimer.Stop()
        mediaRing.Stop()

        imgPhoneDown.Visibility = Visibility.Hidden
        imgPhoneLining.Visibility = Visibility.Hidden

        imgShadowHand.Visibility = Visibility.Visible
        imgPickedUp.Visibility = Visibility.Visible
        imgSpeaking.Visibility = Visibility.Visible

        Dim startLeft = Canvas.GetLeft(imgSpeaking)
        speakMinLeft = startLeft - 10
        speakMaxLeft = startLeft + 10
        speakTimer.Start()

        StartConversation()
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
    ' Conversation sequence
    ' -------------------------
    Private Async Sub StartConversation()
        Await Task.Delay(2500)

        imgLadyClosed.Visibility = Visibility.Visible
        imgMCClosed.Visibility = Visibility.Visible

        Await Task.WhenAll(
            FadeSlideIn(imgLadyClosed, Canvas.GetLeft(imgLadyClosed), Canvas.GetTop(imgLadyClosed)),
            FadeSlideIn(imgMCClosed, Canvas.GetLeft(imgMCClosed), Canvas.GetTop(imgMCClosed))
        )

        mediaConvo.Source = New Uri(System.IO.Path.GetFullPath("pickytures/convo.mp3"))
        mediaConvo.Volume = 0.7
        mediaConvo.Play()

        Dim startTime = DateTime.Now
        Dim flip = False
        While (DateTime.Now - startTime).TotalMilliseconds < 7000
            flip = Not flip
            imgMCClosed.Visibility = If(flip, Visibility.Hidden, Visibility.Visible)
            imgMCOpen.Visibility = If(flip, Visibility.Visible, Visibility.Hidden)
            imgLadyClosed.Visibility = If(flip, Visibility.Visible, Visibility.Hidden)
            imgLadyOpen.Visibility = If(flip, Visibility.Hidden, Visibility.Visible)
            Await Task.Delay(300)
        End While

        ' --- NEW: 1 second delay before switching to MC_MO ---
        Await Task.Delay(1000)

        imgMCClosed.Visibility = Visibility.Hidden
        imgMCOpen.Visibility = Visibility.Hidden
        imgMCOpen.Visibility = Visibility.Visible


        imgMCText.Visibility = Visibility.Visible
        mediaConvo.Stop()
        Await Task.Delay(2000) ' wait 2 seconds before final video/audio

        ' -------------------------
        ' Play final video/audio
        ' -------------------------
        mediaVideo.Source = New Uri(System.IO.Path.GetFullPath("pickytures/run.mp4"))

        ' FULLSCREEN SETTINGS FOR RUN.MP4
        mediaVideo.Width = 900
        mediaVideo.Height = 600
        Canvas.SetLeft(mediaVideo, 0)
        Canvas.SetTop(mediaVideo, 0)
        mediaVideo.Stretch = Stretch.UniformToFill

        ' FORCE VIDEO TO TOP
        Panel.SetZIndex(mediaVideo, 9999)
        mediaVideo.Visibility = Visibility.Visible

        mediaConvo.Source = New Uri(System.IO.Path.GetFullPath("pickytures/heels.mp3"))
        mediaConvo.Volume = 0.7
        mediaConvo.Play()



        mediaConvo.Source = New Uri(System.IO.Path.GetFullPath("pickytures/heels.mp3"))
        mediaConvo.Volume = 0.7
        mediaConvo.Play()

        ' Wait for video to finish
        Dim videoDuration As TimeSpan =
    If(mediaVideo.NaturalDuration.HasTimeSpan,
       mediaVideo.NaturalDuration.TimeSpan,
       TimeSpan.FromSeconds(5))
        mediaVideo.Play()
        Await Task.Delay(videoDuration)

        ' Immediately cover the screen with black so no background flashes
        imgBlackOpen_Copy.Visibility = Visibility.Visible
        Panel.SetZIndex(imgBlackOpen_Copy, 9000)

        ' Now safely hide the video
        mediaVideo.Visibility = Visibility.Hidden
        mediaConvo.Stop()


        ' -------------------------
        ' Fade into final.png
        ' -------------------------
        Await Task.Delay(2000)
        imgMCText.Visibility = Visibility.Hidden
        imgLadyClosed.Visibility = Visibility.Hidden
        imgLadyOpen.Visibility = Visibility.Hidden
        imgMCClosed.Visibility = Visibility.Hidden
        imgMCOpen.Visibility = Visibility.Hidden
        imgBlackOpen_Copy.Visibility = Visibility.Visible
        Panel.SetZIndex(imgBlackOpen_Copy, 1000) ' black copy above everything except final

        imgFinal.Visibility = Visibility.Visible
        Panel.SetZIndex(imgFinal, 1001) ' final.png ABOVE black copy
        imgFinal.Opacity = 0

        For i = 1 To 20
            imgFinal.Opacity += 0.05
            Await Task.Delay(25)
        Next

        ' Hold final.png for 5 seconds
        Await Task.Delay(5000)

        imgFinal.Visibility = Visibility.Hidden
        ResetGame()
    End Sub

    ' -------------------------
    ' Fade/Slide helper
    ' -------------------------
    Private Async Function FadeSlideIn(img As Image, finalLeft As Double, finalTop As Double) As Task
        img.Visibility = Visibility.Visible
        img.Opacity = 0
        Dim startTop = finalTop - 50
        Canvas.SetTop(img, startTop)

        For i = 0 To 20
            img.Opacity += 0.05
            Canvas.SetTop(img, startTop + (50 * (i / 20)))
            Await Task.Delay(25)
        Next

        Canvas.SetTop(img, finalTop)
        img.Opacity = 1
    End Function

    ' -------------------------
    ' Reset Game
    ' -------------------------
    Private Sub ResetGame()
        speakTimer.Stop()
        phonePickedUp = False

        mediaBooVideo.Visibility = Visibility.Hidden
        mediaBooVideo.Stop()


        imgPhoneDown.Visibility = Visibility.Hidden
        imgPhoneLining.Visibility = Visibility.Hidden
        imgPickedUp.Visibility = Visibility.Hidden
        imgSpeaking.Visibility = Visibility.Hidden
        imgShadowHand.Visibility = Visibility.Hidden
        imgFinal.Visibility = Visibility.Hidden
        imgMCText.Visibility = Visibility.Hidden
        imgLadyClosed.Visibility = Visibility.Hidden
        imgLadyOpen.Visibility = Visibility.Hidden
        imgMCClosed.Visibility = Visibility.Hidden
        imgMCOpen.Visibility = Visibility.Hidden
        imgBlackOpen_Copy.Visibility = Visibility.Hidden

        imgBlackOpen.Opacity = 1
        imgBlackOpen.Visibility = Visibility.Visible

        Panel.SetZIndex(mediaVideo, 0)
        Panel.SetZIndex(imgFinal, 0)
        Panel.SetZIndex(imgBlackOpen_Copy, 0)

    End Sub

    ' -------------------------
    ' Secret button click
    ' -------------------------
    Private Sub SecretClicked(sender As Object, e As RoutedEventArgs)
        secretClickCount += 1
        If secretClickCount = 3 Then
            secretClickCount = 0
            Dim rnd As New Random()
            Dim index As Integer = rnd.Next(0, booVideos.Count)

            mediaBooVideo.Source = New Uri(booVideos(index))

            ' Correct size + position (matches button)
            mediaBooVideo.Width = 230
            mediaBooVideo.Height = 169
            Canvas.SetLeft(mediaBooVideo, 300)
            Canvas.SetTop(mediaBooVideo, 131)

            ' Layer: only above background + button
            Panel.SetZIndex(mediaBooVideo, 1)

            mediaBooVideo.Visibility = Visibility.Visible
            mediaBooVideo.Play()
        End If
    End Sub


End Class