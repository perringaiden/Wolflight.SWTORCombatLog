''' <summary>
''' Name: Wolflight.SWTORCombat.LogParser
''' Author: perringaiden
''' E-mail: perrin@techie.com
''' Master Repository: https://github.com/perringaiden/Wolflight.SWTORCombatLog
''' Description:  The Log Parser is a static combat log entry parser.  It has three main methods of parsing, either 
''' by supplying a single line (<see cref="LogParser.GetEntryFromLine"/>), by providing a <see cref="System.IO.StreamReader"/> 
''' and a <see cref="List(Of Wolflight.SWTORCombatLog.LogEntry)"/> for population (<see cref="LogParser.ParseStream"/>), or 
''' by supplying a <see cref="System.IO.StreamReader"/> to receive a <see cref="List(Of Wolflight.SWTORCombatLog.LogEntry)"/>
''' (<see cref="LogParser.ParseStreamToList"/>).
''' </summary>
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

    ' Prevent instancing
    Private Sub New()
        '
    End Sub

    Shared Sub New()
        '
    End Sub

#Region "Stream Reading"

    ''' <summary>
    ''' Reads the combat log entries from a stream of text, and returns a <see cref="List(Of Wolflight.SWTORCombatLog.LogEntry)"/>
    ''' with the parsed combat log entries.
    ''' </summary>
    ''' <param name="aStreamReader">The <see cref="System.IO.StreamReader"/> to read from.</param>
    ''' <returns>A <see cref="List(Of  Wolflight.SWTORCombatLog.LogEntry)"/> containing the parsed combat log entries.</returns>
    Public Shared Function ParseStreamToList(ByVal aStreamReader As System.IO.StreamReader) As List(Of LogEntry)
        Dim rc As New List(Of LogEntry)


        ParseStream(aStreamReader, rc)

        Return rc
    End Function


    ''' <summary>
    ''' Reads the combat log entries from a stream of text, and populates a <see cref="List(Of Wolflight.SWTORCombatLog.LogEntry)"/>
    ''' with the parsed combat log entries.
    ''' </summary>
    ''' <param name="aStreamReader">The <see cref="System.IO.StreamReader"/> to read from.</param>
    ''' <param name="aList">The <see cref="List(Of  Wolflight.SWTORCombatLog.LogEntry)"/> to populate.</param>    
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

#End Region

#Region "Line Reading"

    ''' <summary>
    ''' Reads the <see cref="WolfLight.SWTORCombatLog.LogEntry"/> from a single line of a combat log.
    ''' </summary>
    ''' <param name="aLine">The line to parse.</param>
    ''' <returns>A <see cref="WolfLight.SWTORCombatLog.LogEntry"/> if the line could be parsed, otherwise <see langword="Nothing"/>.</returns>    
    Public Shared Function GetEntryFromLine(ByVal aLine As String) As LogEntry
        Dim rc As LogEntry = New LogEntry
        Dim matches As System.Text.RegularExpressions.MatchCollection


        matches = GetElements(aLine)

        If matches.Count >= 6 Then
            Dim ident As Identifier?

            ' First Element - DateTime
            Dim timestamp As DateTime? = ReadDateElement(matches.Item(0).Value)


            If timestamp.HasValue Then
                rc.Timestamp = timestamp.Value
            End If

            ' Second Element - Source
            ident = ReadIdentifierElement(matches.Item(1).Value)

            If ident.HasValue Then
                rc.Source = ident.Value
                ident = Nothing
            Else
                ' The source must be specified
                Return Nothing
            End If

            ' Third Element - Target
            ident = ReadIdentifierElement(matches.Item(2).Value)

            If ident.HasValue Then
                rc.Target = ident.Value
                ident = Nothing
            Else
                ' The target must be specified
                Return Nothing
            End If

            ' Fourth Element - Ability
            ident = ReadIdentifierElement(matches.Item(3).Value)

            If ident.HasValue Then
                rc.Ability = ident.Value
                ident = Nothing
            End If

            ' Fifth Element - Action and Effect (Muuulti-ident)
            Dim action As Identifier? = Nothing
            Dim affected As Identifier? = Nothing

            Dim parts() As String = matches.Item(4).Value.Split(":"c)


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

            ' Sixth Element
            Dim value As ValueData?


            value = ReadValueData(matches.Item(5).Value)

            If value.HasValue Then
                rc.Value = value.Value
                value = Nothing
            End If

            ' Seventh Element - Threat (Optional)
            If matches.Count >= 7 Then
                Dim threat As Integer?


                threat = ReadThreatData(matches.Item(6).Value)

                If threat.HasValue Then
                    rc.ThreatGain = threat.Value
                    threat = Nothing
                End If
            End If

        End If

        Return rc
    End Function

#End Region

#Region "String Reading"

    Private Shared Function GetElements(ByVal aString As String) As System.Text.RegularExpressions.MatchCollection
        Dim rg As System.Text.RegularExpressions.Regex


        rg = New System.Text.RegularExpressions.Regex("[\[\(\<].*?[\]\)\>]")

        Return rg.Matches(aString)
    End Function

#End Region


#Region "Element Reading"

    ''' <summary>
    ''' Removes the boundaries from an Element entry.
    ''' </summary>
    ''' <param name="aValue">The element to strip boundaries from.</param>
    ''' <returns>The tidied element entry, or <see langword="Nothing"/> if <paramref name="aValue"/> was  <see langword="Nothing"/>.</returns>
    ''' <remarks>Format: [Name {ID}] or [@name] or [datestring] with or without square brackets.</remarks>
    Private Shared Function StripElementBoundaries(ByVal aValue As String) As String
        If aValue IsNot Nothing Then
            Return aValue.Trim.TrimStart("["c).TrimEnd("]"c).Trim
        Else
            Return Nothing
        End If
    End Function


    ''' <summary>
    ''' Removes the boundaries from an Value entry.
    ''' </summary>
    ''' <param name="aValue">The value to strip boundaries from.</param>
    ''' <returns>The tidied value entry, or <see langword="Nothing"/> if <paramref name="aValue"/> was  <see langword="Nothing"/>.</returns>
    ''' <remarks>Format: (Amount Type {ID} or (Amount) with or without brackets.</remarks>
    Private Shared Function StripValueBoundaries(ByVal aValue As String) As String
        If aValue IsNot Nothing Then
            Return aValue.Trim.TrimStart("("c).TrimEnd(")"c).Trim
        Else
            Return Nothing
        End If
    End Function


    ''' <summary>
    ''' Removes the boundaries from an Threat entry.
    ''' </summary>
    ''' <param name="aValue">The threat entry to strip boundaries from.</param>
    ''' <returns>The tidied threat entry, or <see langword="Nothing"/> if <paramref name="aValue"/> was  <see langword="Nothing"/>.</returns>
    Private Shared Function StripThreatBoundaries(ByVal aValue As String) As String
        If aValue IsNot Nothing Then
            Return aValue.Trim.TrimStart("<"c).TrimEnd(">"c).Trim
        Else
            Return Nothing
        End If
    End Function


    ''' <summary>
    ''' Reads a US-formatted date/time string from an element.
    ''' </summary>
    ''' <param name="aText">The text to read.</param>
    ''' <returns>A <see cref="Nullable(Of DateTime)" /> with a value if the string could be read, otherwise <see langword="Nothing"/>.</returns>
    Private Shared Function ReadDateElement(ByVal aText As String) As DateTime?
        Dim rc As DateTime


        aText = StripElementBoundaries(aText)

        If String.IsNullOrEmpty(aText) = False Then
            If DateTime.TryParse( _
                        aText, _
                        System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), _
                        Globalization.DateTimeStyles.None, rc _
                    ) Then

                Return rc
            End If
        End If

        Return Nothing

    End Function


    ''' <summary>
    ''' Reads the components of a value from a provided <paramref name="aText"/>
    ''' <see cref="String" /> and generates a <see cref="WolfLight.SWTORCombatLog.Identifier"/>
    ''' object.
    ''' </summary>
    ''' <param name="aText">The string to parse for value data.</param>
    ''' <returns>
    ''' A <see cref="Nullable(Of Wolflight.SWTORCombatLog.Identifier)"/> which has a value if
    ''' the text could be parsed.
    ''' </returns>
    ''' <remarks>Format: [Name {ID}] or [@name] or [datestring]</remarks>
    Private Shared Function ReadIdentifierElement(ByVal aText As String) As Identifier?
        ' Text: [Elite Tastybobble {846623953387520}]
        ' Player: [@Idrurrez]
        aText = StripElementBoundaries(aText)

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
    ''' <remarks>Format: (250* energy {836045448940874})</remarks>
    Private Shared Function ReadValueData(ByVal aText As String) As ValueData?
        ' (250* energy {836045448940874})
        aText = StripValueBoundaries(aText)

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


                If aText(endValueIndex - 1) = "*"c Then
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


    ''' <summary>
    ''' Reads an integer from a threat entry.
    ''' </summary>
    ''' <param name="aText">The text to read.</param>
    ''' <returns>A <see cref="Nullable(Of Integer)" /> with a value if the string could be read, otherwise <see langword="Nothing"/>.</returns>
    Private Shared Function ReadThreatData(ByVal aText As String) As Integer?
        aText = StripThreatBoundaries(aText)

        If String.IsNullOrEmpty(aText) = False Then
            Dim threatvalue As Integer = 0


            If Integer.TryParse(aText, threatvalue) Then
                Return threatvalue
            End If
        End If

        Return Nothing
    End Function

#End Region

End Class
