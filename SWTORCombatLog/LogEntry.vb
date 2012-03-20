''' <summary>
''' Name: Wolflight.SWTORCombat.LogEntry
''' Author: perringaiden
''' E-mail: perrin@techie.com
''' Master Repository: https://github.com/perringaiden/Wolflight.SWTORCombatLog
''' Description:  The <see cref="LogEntry"/> class is used to house the data for a single
''' combat log line, decomposed into its separate parts.
''' </summary>
Public Class LogEntry

    ''' <summary>
    ''' The time at which the combat log entry occurred.
    ''' </summary>
    ''' <returns>The time at which the combat log entry occurred.</returns>
    Public Property Timestamp As DateTime

    ''' <summary>
    ''' The <see cref="Identifier"/> of the source of the combat log event.
    ''' </summary>
    ''' <returns>The <see cref="Identifier"/> of the source of the combat log event.</returns>
    Public Property Source As Identifier

    ''' <summary>
    ''' The <see cref="Identifier"/> of the target of the combat log event.
    ''' </summary>
    ''' <returns>The <see cref="Identifier"/> of the target of the combat log event.</returns>
    Public Property Target As Identifier

    ''' <summary>
    ''' The optional <see cref="Identifier"/> of the ability used in the combat log event.
    ''' </summary>
    ''' <returns>The optional <see cref="Identifier"/> of the ability used in the combat log event.</returns>
    Public Property Ability As Identifier ' The name of the ability used - Force Surge, Kolto Pack

    ''' <summary>
    ''' The <see cref="Identifier"/> of the action that caused in the combat log event.
    ''' </summary>
    ''' <returns>The <see cref="Identifier"/> of the action that caused the combat log event.</returns>
    Public Property Action As Identifier ' The action that occurred - Spend (resource), Restore (resource), Apply Effect etc.

    ''' <summary>
    ''' The <see cref="Identifier"/> of the element that was affected by the <see cref="Action"/>.
    ''' </summary>
    ''' <returns>The <see cref="Identifier"/> of the element that was affected by the <see cref="Action"/>.</returns>
    Public Property AffectedElement As Identifier ' The thing affected - A buff, health point, force

    ''' <summary>
    ''' The <see cref="ValueData"/> of the combat log event.
    ''' </summary>
    ''' <returns>The <see cref="ValueData"/> of the combat log event.</returns>
    Public Property Value As ValueData ' - The value of whatever occurred (optional)

    ''' <summary>
    ''' The amount of Threat gained by this combat log event.
    ''' </summary>
    ''' <returns>The amount of Threat gained by this combat log event.</returns>
    Public Property ThreatGain As Integer ' - Threat generated

End Class

''' <summary>
''' Name: Wolflight.SWTORCombat.ValueData
''' Author: perringaiden
''' E-mail: perrin@techie.com
''' Master Repository: https://github.com/perringaiden/Wolflight.SWTORCombatLog
''' Description:  The <see cref="ValueData"/> class is used to house the value of a single
''' combat log line, decomposed into its separate parts.
''' </summary>
Public Structure ValueData

    ''' <summary>
    ''' The numerical amount that the <see cref="ValueData"/> represents.
    ''' </summary>
    ''' <returns>The numerical amount that the <see cref="ValueData"/> represents.</returns>
    Public Property Amount As Integer

    ''' <summary>
    ''' An <see cref="Identifier"/> that indicates what type the value is, for example 'physical' or 'energy'.
    ''' </summary>
    ''' <returns>An <see cref="Identifier"/> that indicates what type the value is.</returns>
    Public Property ValueType As Identifier

    ''' <summary>
    ''' A flag indicating that the <see cref="ValueData"/> represents a Critical value.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the <see cref="ValueData"/> represents a Critical value; otherwise False.</returns>
    Public Property Critical As Boolean

End Structure


''' <summary>
''' Name: Wolflight.SWTORCombat.Identifier
''' Author: perringaiden
''' E-mail: perrin@techie.com
''' Master Repository: https://github.com/perringaiden/Wolflight.SWTORCombatLog
''' Description:  The <see cref="Identifier"/> class is used to house a unique identifier
''' and a textual description of the identified entity.
''' </summary>
Public Structure Identifier
    Public Property ID As UInt64
    Public Property DisplayName As String
    Public Property IsPlayer As Boolean
End Structure
