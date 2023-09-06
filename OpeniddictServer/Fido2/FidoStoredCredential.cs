using Fido2NetLib.Objects;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using OpeniddictServer.Fido2;

namespace Fido2Identity;

/// <summary>
/// Represents a WebAuthn credential.
/// </summary>
public class FidoStoredCredential
{
    /// <summary>
    /// Gets or sets the primary key for this user.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual int Id { get; set; }

    public virtual byte[]? CredentialId { get; set; }

    public virtual PublicKeyCredentialType Type { get; set; } = PublicKeyCredentialType.PublicKey;

    /// <summary>
    /// Gets or sets the user name for this user.
    /// </summary>
    public virtual string? UserName { get; set; }

    public virtual byte[]? UserId { get; set; } 

    /// <summary>
    /// Gets or sets the public key for this user.
    /// </summary>
    public virtual byte[]? PublicKey { get; set; }

    /// <summary>
    /// Gets or sets the user handle for this user.
    /// </summary>
    public virtual byte[]? UserHandle { get; set; }

    public virtual uint SignCount { get; set; }

    /// <summary>
    /// The value returned from getTransports() when the public key credential source was registered.
    /// </summary>
    [NotMapped]
    public AuthenticatorTransport[] Transports
    {
        get => string.IsNullOrWhiteSpace(TransportsJson) ? null : JsonSerializer.Deserialize<AuthenticatorTransport[]>(TransportsJson);
        set => TransportsJson = JsonSerializer.Serialize(value);
    }

    public virtual string? TransportsJson { get; set; }

    /// <summary>
    /// The value of the BE flag when the public key credential source was created.
    /// </summary>
    public virtual bool BE { get; set; }

    /// <summary>
    /// The latest value of the BS flag in the authenticator data from any ceremony using the public key credential source.
    /// </summary>
    public virtual bool BS { get; set; }

    /// <summary>
    /// The value of the attestationObject attribute when the public key credential source was registered. 
    /// Storing this enables the Relying Party to reference the credential's attestation statement at a later time.
    /// </summary>
    public virtual byte[] AttestationObject { get; set; }

    /// <summary>
    /// The value of the clientDataJSON attribute when the public key credential source was registered. 
    /// Storing this in combination with the above attestationObject item enables the Relying Party to re-verify the attestation signature at a later time.
    /// </summary>
    public virtual byte[] AttestationClientDataJson { get; set; }

    [NotMapped]
    public List<byte[]> DevicePublicKeys
    {
        get => string.IsNullOrWhiteSpace(DevicePublicKeysJson) ? new List<byte[]>() : JsonSerializer.Deserialize<List<string>>(DevicePublicKeysJson).FromListOfBase64String();
        set
        {
            var valueAsListOfBase64Sttring = value.ToListOfBase64String();
            DevicePublicKeysJson = valueAsListOfBase64Sttring is null ? "" : JsonSerializer.Serialize(valueAsListOfBase64Sttring); 
        }
    }

    public virtual string? DevicePublicKeysJson { get; set; }

    public virtual string? CredType { get; set; }
    
    /// <summary>
    /// Gets or sets the registration date for this user.
    /// </summary>
    public virtual DateTime RegDate { get; set; }

    /// <summary>
    /// Gets or sets the Authenticator Attestation GUID (AAGUID) for this user.
    /// </summary>
    /// <remarks>
    /// An AAGUID is a 128-bit identifier indicating the type of the authenticator.
    /// </remarks>
    public virtual Guid AaGuid { get; set; }

    [NotMapped]
    public PublicKeyCredentialDescriptor? Descriptor
    {
        get => string.IsNullOrWhiteSpace(DescriptorJson) ? null : JsonSerializer.Deserialize<PublicKeyCredentialDescriptor>(DescriptorJson);
        set => DescriptorJson = value is null ? "" : JsonSerializer.Serialize(value);
    }

    public virtual string? DescriptorJson { get; set; }

    public virtual bool IsPasskey { get; set; }

    
}



