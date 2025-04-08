using JasperFx.Core;
using Wolverine;
using Identifications.Features;
using Wolverine.Http;

namespace Identifications;
public record StartIdentificationFlow;

public class IdentificationSimpleFlow : Saga
{
    public Guid Id { get; set; }
    
    public string[] DocumentTypes { get; set; }
    
    public record NewIdentificationResponse(Guid IdentificationId) : CreationResponse("/api/identifications/" + IdentificationId);
    public record RegisterIdentificationRequest(Guid ApplicantId, string[] DocumentTypes);
        
    [WolverinePost("api/identifications")]
    public static (NewIdentificationResponse, IdentificationSimpleFlow, OutgoingMessages) Start(RegisterIdentificationRequest request)
    {
        var identificationId = CombGuidIdGeneration.NewGuid();
        
        return (
            new NewIdentificationResponse(identificationId),
            new IdentificationSimpleFlow{ Id = identificationId, DocumentTypes = request.DocumentTypes },
            [new RegisterIdentification.Command(identificationId, request.ApplicantId)]
        );
    }

    // public static OutgoingMessages Handle(RequestData requestData) =>
    //     [new IdentificationCommand.CollectData.WaitForSelfie { IdentificationId = requestData.Id }];
    //
    // public static OutgoingMessages Handle(IdentificationCommand.CollectData.RegisterSelfie collectingDataRequested) =>
    //     [new IdentificationCommand.CollectData.RegisterSelfie() { IdentificationId = collectingDataRequested.Id }];
    //
    // public static OutgoingMessages Handle(IdentificationEvent.CollectingDataEvent.SelfieReceived collectingDataRequested) =>
    //     [new IdentificationCommand.CollectData.WaitForPassport { IdentificationId = collectingDataRequested.Id }];
    //
    // public static OutgoingMessages Handle(IdentificationCommand.CollectData.RegisterPassport collectingDataRequested) =>
    //     [new IdentificationCommand.CollectData.RegisterPassport { IdentificationId = collectingDataRequested.Id }];
    //
    // public static OutgoingMessages Handle(IdentificationEvent.CollectingDataEvent.PassportReceived collectingDataRequested) =>
    //     [new IdentificationCommand.CollectData.WaitForAddress { IdentificationId = collectingDataRequested.Id }];
    //
    // public static OutgoingMessages Handle(IdentificationCommand.CollectData.RegisterAddress collectingDataRequested) =>
    //     [new IdentificationCommand.CollectData.RegisterAddress() { IdentificationId = collectingDataRequested.Id }];
    //
    // public static OutgoingMessages Handle(IdentificationEvent.CollectingDataEvent.AddressReceived collectingDataRequested) =>
    //     [new IdentificationCommand.ProcessData(collectingDataRequested.Id)];
    //
    // public static OutgoingMessages Handle(IdentificationEvent.CollectingDataFailed collectingDataFailed) =>
    //     [new IdentificationCommand.Fail { IdentificationId = collectingDataFailed.Id }];
}