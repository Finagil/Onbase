Imports System.IO
Imports System.Net.Mail

Module OnBase
    Dim Arg() As String
    Sub Main()
        Dim fecha1 As Date = Date.Now
        Arg = Environment.GetCommandLineArgs()
        If Arg.Length > 1 Then
            Select Case Val(Arg(1))
                Case 0
                    LLenaSQL(False)
                Case 1
                    LLenaSQL(True)
            End Select
            recursivoDir(My.Settings.RutaOrigen)
        End If

        Dim fecha2 As Date = Date.Now
        Dim x As Long = DateDiff(DateInterval.Minute, fecha1, fecha2)
        Console.WriteLine("Minutos: {0}", x)
    End Sub

    Private Sub recursivoDir(ByVal elDirectorio As String)
        Console.WriteLine("Los subdirectorios de {0}", elDirectorio)
        recorrerDir(elDirectorio, 0)
    End Sub

    Private Sub recorrerDir(ByVal elDir As String, ByVal nivel As Integer)
        ' La sangría del nivel examinado
        Try

        
        Dim sangria As String = New String(" "c, nivel)
        Dim RutaNueva As String = My.Settings.RutaDestino
        Dim infoReader1 As System.IO.FileInfo
            Dim infoReader2 As System.IO.FileInfo
            Dim r As String = ""
            Dim rr As String = ""


        ' Los subdirectorios del directorio indicado
        Dim directorios As String() = Directory.GetDirectories(elDir)
        Console.Write("{0}Directorio {1} con {2} subdirectorios", sangria, elDir, directorios.Length)
        Dim ficheros As String() = Directory.GetFiles(elDir)
        Console.WriteLine(" y {0} ficheros", ficheros.Length)

        ' Si tiene subdirectorios, recorrerlos
        If directorios.Length > 0 Then
            For Each d As String In directorios
                ' Llamar de forma recursiva a este mismo método
                    'crea las carpetas
                    r = RutaNueva & Mid(d, 21, d.Length)
                    If Not Directory.Exists(r) Then
                        Directory.CreateDirectory(r)
                    End If
                recorrerDir(d, nivel + 2)
            Next
        End If

        ' Si tiene archivos, recorrerlos
        If ficheros.Length > 0 Then
            For Each f As String In ficheros
                'copia los archivos
                    If InStr(UCase(f), ".JPG") > 0 Or InStr(UCase(f), ".TIF") > 0 Or InStr(UCase(f), ".PDF") > 0 Then
                        rr = RutaNueva & Mid(f, 21, f.Length)
                        If Not File.Exists(rr) Then
                            File.Copy(f, rr, True)
                        Else
                            infoReader1 = My.Computer.FileSystem.GetFileInfo(f)
                            infoReader2 = My.Computer.FileSystem.GetFileInfo(rr)
                            If infoReader1.Length <> infoReader2.Length Then
                                File.Copy(f, rr, True)
                            End If
                        End If
                    End If
                Console.WriteLine("       Ficheros: {0}", f)
            Next
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            EnviaError("Ecacerest@lamoderna.com.mx", ex.Message, "Error de Onbase " & Date.Now)
        End Try
    End Sub

    Private Sub LLenaSQL(Todo As Boolean)
        Try
            Dim taOnBase As New OnBaseDSTableAdapters.DatosOnBaseTableAdapter
            Dim ta As New FinagilDSTableAdapters.OnBaseTableAdapter
            Dim t As New OnBaseDS.DatosOnBaseDataTable
            If Todo = True Then
                ta.DeleteTODO()
                taOnBase.FillByTODO(t)
            Else
                ta.DeleteAll()
                taOnBase.Fill(t)
            End If

            For Each r As OnBaseDS.DatosOnBaseRow In t.Rows
                ta.Insert(r.itemnum, r.itemname, r.itemdate, r.itempagenum, r.filepath)
                Console.WriteLine(r.itemnum)
            Next
            Console.WriteLine("Borrando archivos de no imagenes")
            ta.DeleteNoImagenes()
            Console.WriteLine("Actualiza Documentos")
            ta.UpdateDocumentos()
            Console.WriteLine("Actualiza Datos 1")
            ta.UpdateLlenaDatos1()
            Console.WriteLine("Actualiza Datos 2")
            ta.UpdateLlenaDatos2()
            Console.WriteLine("Actualiza Datos 3")
            ta.UpdateLlenaDatos3()
        Catch ex As Exception
            EnviaError("Ecacerest@lamoderna.com.mx", ex.Message, "error de Onbase")
        End Try
    End Sub

    Private Sub EnviaError(ByVal Para As String, ByVal Mensaje As String, ByVal Asunto As String)
        If InStr(Mensaje, Asunto) = 0 Then
            Dim Mensage As New MailMessage("InternoBI2008@cmoderna.com", Trim(Para), Trim(Asunto), Mensaje)
            Dim Cliente As New SmtpClient(My.Settings.SMTP, My.Settings.SMTP_port)
            Try
                Dim Credenciales As String() = My.Settings.SMTP_creden.Split(",")
                Cliente.Credentials = New System.Net.NetworkCredential(Credenciales(0), Credenciales(1), Credenciales(2))
                Cliente.Send(Mensage)
            Catch ex As Exception
                'ReportError(ex)
            End Try
        Else
            Console.WriteLine("No se ha encontrado la ruta de acceso de la red")
        End If
    End Sub
End Module
