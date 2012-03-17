
Public Class LogEntry

    Public Property Timestamp As DateTime
    Public Property Source As Identifier
    Public Property Target As Identifier
    Public Property Ability As Identifier ' The name of the ability used - Force Surge, Kolto Pack
    Public Property Action As Identifier ' The action that occurred - Spend (resource), Restore (resource), Apply Effect etc.
    Public Property AffectedElement As Identifier ' The thing affected - A buff, health point, force
    Public Property Value As ValueData ' - The value of whatever occurred (optional)
    Public Property ThreatGain As Integer ' - Threat generated

End Class

Public Structure ValueData
    Public Property Amount As Integer
    Public Property ValueType As Identifier
    Public Property Critical As Boolean
End Structure

Public Structure Identifier
    Public Property ID As UInt64
    Public Property DisplayName As String
    Public Property IsPlayer As Boolean
End Structure
