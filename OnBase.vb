﻿Imports System.IO
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
            recursivoDirTMP(My.Settings.RutaOrigen, My.Settings.RutaDestino)
            'recursivoDirTMP(My.Settings.RutaTmpOrg, My.Settings.RutaTmpDest)
        End If

        Dim fecha2 As Date = Date.Now
        Dim x As Long = DateDiff(DateInterval.Minute, fecha1, fecha2)
        Console.WriteLine("Minutos: {0}", x)
    End Sub

    Private Sub recursivoDirTMP(ByVal elDirectorio As String, Destino As String)
        Console.WriteLine("Los subdirectorios de {0}", elDirectorio)
        recorrerDirTMP(elDirectorio, 0, Destino)
    End Sub

    Private Sub LLenaSQL(Todo As Boolean)
        Try
            Dim taOnBase As New OnBaseDSTableAdapters.DatosOnBaseTableAdapter
            Dim ta As New FinagilDSTableAdapters.OnBaseTableAdapter
            Dim t As New OnBaseDS.DatosOnBaseDataTable
            ta.TimeOuts = 120
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
            Console.WriteLine("Actualiza Datos 0")
            ta.UpdateLlenaDatos0()
            Console.WriteLine("Actualiza Datos 1")
            ta.UpdateLlenaDatos1()
            Console.WriteLine("Actualiza Datos 2")
            ta.UpdateLlenaDatos2()
            Console.WriteLine("Actualiza Datos 3")
            ta.UpdateLlenaDatos3()
        Catch ex As Exception
            EnviaError("ecacerest@cmoderna.com.mx", ex.Message, "error de Onbase")
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

    Private Sub recorrerDirTMP(ByVal elDir As String, ByVal nivel As Integer, RutaNueva As String)
        ' La sangría del nivel examinado
        Try
            Dim sangria As String = New String(" "c, nivel)
            Dim infoReader1 As System.IO.FileInfo
            Dim infoReader2 As System.IO.FileInfo
            Dim r As String = ""
            Dim rr As String = ""
            Dim LargoR As Integer = elDir.Length


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
                    r = RutaNueva & Mid(d, LargoR, d.Length) & "\"
                    If Not Directory.Exists(r) Then
                        Directory.CreateDirectory(r)
                    End If
                    recorrerDirTMP(d & "\", nivel + 2, r)
                Next
            End If

            ' Si tiene archivos, recorrerlos
            If ficheros.Length > 0 Then
                For Each f As String In ficheros
                    'copia los archivos
                    If InStr(UCase(f), ".ID") > 0 Then
                        Continue For
                    End If
                    rr = RutaNueva & Mid(f, LargoR, f.Length)
                    If Not File.Exists(rr) Then
                        File.Copy(f, rr, True)
                    Else
                        infoReader1 = My.Computer.FileSystem.GetFileInfo(f)
                        infoReader2 = My.Computer.FileSystem.GetFileInfo(rr)
                        If infoReader1.Length <> infoReader2.Length Then
                            File.Copy(f, rr, True)
                        End If
                    End If
                    'End If
                    Console.WriteLine("       Ficheros: {0}", f)
                Next
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            EnviaError("Ecacerest@lamoderna.com.mx", ex.Message, "Error de Onbase " & Date.Now)
        End Try
    End Sub

End Module
