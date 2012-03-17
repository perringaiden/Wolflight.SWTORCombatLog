Public Class LogParser

    Private Const CG_OPENELEMENT As Char = "["c
    Private Const CG_CLOSEELEMENT As Char = "]"c
    Private Const CG_OPENVALUE As Char = "("c
    Private Const CG_CLOSEVALUE As Char = ")"c
    Private Const CG_OPENTHREAT As Char = "<"c
    Private Const CG_CLOSETHREAT As Char = ">"c
    Private Const CG_OPENIDENTIFIER As Char = "{"c
    Private Const CG_CLOSEIDENTIFIER As Char = "}"c
    Private Const CG_ELEMENTSPLIT As Char = ":"c
    Private Const CG_PLAYERIDENTIFIER As Char = "@"c

    '[03/01/2012 14:23:52] [Elite Tastybobble {846623953387520}] [@Idrurrez] [Ranged Attack {813449625993216}] [ApplyEffect {836045448945477}: Damage {836045448945501}] (261 energy {836045448940874}) <261>
    '[03/01/2012 14:23:52] [@Idrurrez] [Elite Tastybobble {846623953387520}] [Master Strike {812139660967936}] [ApplyEffect {836045448945477}: Damage {836045448945501}] (874* energy {836045448940874}) <874>


    Public Shared Function ParseStreamToList(ByVal aStreamReader As System.IO.StreamReader) As List(Of LogEntry)
        Dim rc As New List(Of LogEntry)


        ParseStream(aStreamReader, rc)

        Return rc
    End Function

    Public Shared Sub ParseStream(ByVal aStreamReader As System.IO.StreamReader, ByVal aList As List(Of LogEntry))
        If aList IsNot Nothing Then
            Do While aStreamReader.EndOfStream = False
                Dim line As String


                line = aStreamReader.ReadLine

                If String.IsNullOrEmpty(line) = False Then
                    Dim entry As LogEntry


                    entry = GetEntryFromLine(line)

                    If entry IsNot Nothing Then
                        aList.Add(entry)
                    End If
                End If
            Loop
        End If
    End Sub



#Region "Line Reading"

    Public Shared Function GetEntryFromLine(ByVal aLine As String) As LogEntry
        Try
            Dim rc As LogEntry = New LogEntry
            Dim curIdx As Integer = 0
            Dim elemString As String = Nothing


            ' Read the date/timestamp
            elemString = GetNextElementString(aLine, curIdx)

            If String.IsNullOrEmpty(elemString) = False Then
                Dim timestamp As DateTime? = Nothing

                timestamp = ReadDateElement(elemString)

                If timestamp.HasValue Then
                    rc.Timestamp = timestamp.Value
                End If
            Else
                ' Couldn't retrieve the date which is always present.
                Return Nothing
            End If

            ' Read the Source
            elemString = GetNextElementString(aLine, curIdx)

            If String.IsNullOrEmpty(elemString) = False Then
                Dim source As Identifier? = Nothing

                source = ReadIdentifierElement(elemString)

                If source.HasValue Then
                    rc.Source = source.Value
                Else
                    ' The source must be specified
                    Return Nothing
                End If
            End If

            ' Read the Target
            elemString = GetNextElementString(aLine, curIdx)

            If String.IsNullOrEmpty(elemString) = False Then
                Dim target As Identifier? = Nothing

                target = ReadIdentifierElement(elemString)

                If target.HasValue Then
                    rc.Target = target.Value
                Else
                    ' The target must be specified
                    Return Nothing
                End If
            End If

            ' Read the Ability
            elemString = GetNextElementString(aLine, curIdx)

            If String.IsNullOrEmpty(elemString) = False Then
                Dim ability As Identifier? = Nothing

                ability = ReadIdentifierElement(elemString)

                ' The ability can be nothing.
                If ability.HasValue Then
                    rc.Ability = ability.Value
                End If
            End If

            ' Read the action and affected element
            elemString = GetNextElementString(aLine, curIdx)

            If String.IsNullOrEmpty(elemString) = False Then
                Dim action As Identifier? = Nothing
                Dim affected As Identifier? = Nothing

                Dim parts() As String = elemString.Split(":"c)


                If parts.GetLength(0) >= 2 Then
                    action = ReadIdentifierElement(parts(0))
                    affected = ReadIdentifierElement(parts(1))

                    If action.HasValue AndAlso affected.HasValue Then
                        rc.Action = action.Value
                        rc.AffectedElement = affected.Value
                    Else
                        ' There wasn't an action and affected element.
                        Return Nothing
                    End If
                Else
                    ' There wasn't an action and affected element.
                    Return Nothing
                End If
            End If

            ' Read the action and affected element
            elemString = GetNextValueString(aLine, curIdx)

            If String.IsNullOrEmpty(elemString) = False Then
                Dim data As ValueData? = Nothing


                data = ReadValueData(elemString)

                If data.HasValue Then
                    rc.Value = data.Value
                End If
            End If


            ' Read the action and affected element
            elemString = GetNextThreatString(aLine, curIdx)

            If String.IsNullOrEmpty(elemString) = False Then
                Dim threatvalue As Integer = 0


                If Integer.TryParse(elemString, threatvalue) Then
                    rc.ThreatGain = threatvalue
                End If
            End If

            Return rc

        Catch ex As Exception
            Trace.WriteLine("Exception reading line: " & aLine & Environment.NewLine & "Exception: " & ex.ToString)

            Return Nothing
        End Try

    End Function

#End Region

#Region "String Reading"


    Private Shared Function GetNextElementString( _
            ByVal aLine As String, _
            ByRef aCurrentIndex As Integer _
        ) As String

        Return GetNextString(CG_OPENELEMENT, CG_CLOSEELEMENT, aLine, aCurrentIndex)
    End Function


    Private Shared Function GetNextValueString( _
            ByVal aLine As String, _
            ByRef aCurrentIndex As Integer _
        ) As String

        Return GetNextString(CG_OPENVALUE, CG_CLOSEVALUE, aLine, aCurrentIndex)
    End Function


    Private Shared Function GetNextThreatString( _
        ByVal aLine As String, _
        ByRef aCurrentIndex As Integer _
    ) As String

        Return GetNextString(CG_OPENTHREAT, CG_CLOSETHREAT, aLine, aCurrentIndex)
    End Function


    Private Shared Function GetNextString( _
            ByVal aOpenCharacter As Char, _
            ByVal aCloseCharacter As Char, _
            ByVal aLine As String, _
            ByRef aCurrentIndex As Integer _
        ) As String

        Try
            Dim nextStartIndex As Integer = -1
            Dim nextEndIndex As Integer = -1


            nextStartIndex = aLine.IndexOf(aOpenCharacter, aCurrentIndex)

            If nextStartIndex >= aCurrentIndex Then
                Dim source As Identifier? = Nothing


                aCurrentIndex = nextStartIndex + 1
                nextEndIndex = aLine.IndexOf(aCloseCharacter, aCurrentIndex)

                If nextEndIndex >= aCurrentIndex Then
                    ' Return the selected substring
                    Return aLine.Substring(aCurrentIndex, nextEndIndex - aCurrentIndex)
                Else
                    ' There isn't an end character, so move the cursor to the
                    ' end of the line. We do this by prepping the nextEndIndex.
                    nextEndIndex = aLine.Length - 1
                End If

            Else
                ' We didn't find a start element.  However, if this is a broken line we want
                ' to be able to continue, so we need to find the next end so we can move forward
                ' as usual.
                nextEndIndex = aLine.IndexOf(aCloseCharacter, aCurrentIndex)
            End If

            ' Move the index forward past the end of the element.
            aCurrentIndex = nextEndIndex + 1

        Catch ex As ArgumentOutOfRangeException
            '
        End Try

        Return Nothing
    End Function


#End Region

#Region "Element Reading"


    Private Shared Function ReadDateElement(ByVal aText As String) As DateTime?
        Dim rc As DateTime


        Try
            rc = DateTime.Parse(aText, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))

            Return rc

        Catch ex As FormatException
            Return Nothing
        End Try

    End Function


    Private Shared Function ReadIdentifierElement(ByVal aText As String) As Identifier?
        ' Text: Elite Tastybobble {846623953387520}
        ' Player: @Idrurrez


        If String.IsNullOrEmpty(aText) = False Then
            If aText.Chars(0) = CG_PLAYERIDENTIFIER Then
                ' The entire string (trimmed) is the player identifier, and without the @ is the name.
                aText = aText.Trim

                Return New Identifier() With {.ID = 0, .DisplayName = aText.TrimStart(CG_PLAYERIDENTIFIER), .IsPlayer = True}

            Else
                Dim parts() As String = aText.Split(CG_OPENIDENTIFIER)


                If parts.GetLength(0) = 2 Then
                    Dim name As String = parts(0).Trim
                    Dim idEnd As Integer = -1
                    Dim id As UInt64 = 0


                    idEnd = parts(1).IndexOf(CG_CLOSEIDENTIFIER)

                    If idEnd > 0 Then ' Index must be more than empty (-1) or the first character (0)
                        UInt64.TryParse(parts(1).Substring(0, idEnd).Trim, id) ' Extract the text before the identifier close, then remove whitespace.
                    End If

                    If Not String.IsNullOrEmpty(name) Then
                        Return New Identifier() With {.ID = id, .DisplayName = name, .IsPlayer = False}
                    End If

                End If
            End If
        End If

        Return Nothing

    End Function


    ''' <summary>
    ''' Reads the components of a value from a provided <paramref name="aText"/>
    ''' <see cref="String" /> and generates a <see cref="WolfLight.SWTORCombatLog.ValueData"/>
    ''' object.
    ''' </summary>
    ''' <param name="aText">The string to parse for value data.</param>
    ''' <returns>
    ''' A <see cref="Nullable(Of Wolflight.SWTORCombatLog.ValueData)"/> which has a value if
    ''' the text could be parsed.
    ''' </returns>
    ''' <remarks>Format: 250* energy {836045448940874}</remarks>
    Private Shared Function ReadValueData(ByVal aText As String) As ValueData?
        ' 250* energy {836045448940874}

        ' Since we'll be working with spaces as a split character, make sure that
        ' there's not leading or trailing spaces, before we start trying to
        ' work with the text.
        If String.IsNullOrEmpty(aText) = False Then
            aText = aText.Trim
        End If

        ' Now that we've trimmed the text, checkt that we have data.
        If String.IsNullOrEmpty(aText) = False Then
            Dim endValueIndex As Integer


            ' Find the first space
            endValueIndex = aText.IndexOf(" "c)

            If endValueIndex > 0 Then
                ' There's a space, so there's an identifier after it.
                Dim ident As Identifier?
                Dim amount As Integer = 0
                Dim gotAmount As Boolean
                Dim isCrit As Boolean = False


                If aText(endValueIndex) = "*"c Then
                    gotAmount = Integer.TryParse(aText.Substring(0, endValueIndex - 1), amount)
                    isCrit = True
                Else
                    gotAmount = Integer.TryParse(aText.Substring(0, endValueIndex), amount)
                End If



                If gotAmount Then
                    ident = ReadIdentifierElement(aText.Substring(endValueIndex + 1, aText.Length - endValueIndex - 1))

                    If ident.HasValue Then
                        Return New ValueData With {.Amount = amount, .Critical = isCrit, .ValueType = ident.Value}
                    Else
                        Return New ValueData With {.Amount = amount, .Critical = isCrit}
                    End If

                End If

            Else
                ' There's no space, so its just a number.
                Dim dataValue As Integer = 0


                If Integer.TryParse(aText, dataValue) Then
                    Return New ValueData With {.Amount = dataValue}
                End If
            End If
        End If

        Return Nothing
    End Function


#End Region

End Class
