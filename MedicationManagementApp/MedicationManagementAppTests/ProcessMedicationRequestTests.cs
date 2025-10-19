using MedicationManagementApp.Exceptions;
using MedicationManagementApp.Models;
using MedicationManagementApp.Repositories;
using MedicationManagementApp.Services;
using Moq;
using Shouldly;

namespace MedicationManagementAppTests
{
    public class ProcessMedicationRequestTests
    {
        [Fact]
        public async Task MedicationRequest_NotExistingMedRequestID_ShouldThrowNotFoundException()
        {
            var (service, emailMock) = CreateMedicationRequestService();

            await Should.ThrowAsync<MedicationRequestNotFoundException>(() => service.ProcessMedicationRequest(0));
        }

        [Fact]
        public async Task MedicationRequest_ShouldThrowOutOfStockException()
        {
            var (service, emailMock) = CreateMedicationRequestService();

            await Should.ThrowAsync<OutOfStockException>(() => service.ProcessMedicationRequest(1));
        }

        [Fact]
        public async Task MedicationRequest_ShouldthrowInsufficientStockException()
        {
            var (service, emailMock) = CreateMedicationRequestService();

            await Should.ThrowAsync<InsufficientStockException>(() => service.ProcessMedicationRequest(2));
        }

        [Fact]
        public async Task MedicationRequest_ShouldSendNotification_WhenRequestIsSuccessful()
        {
            var (service, emailMock) = CreateMedicationRequestService();

            var result = await service.ProcessMedicationRequest(3);

            result.MedicationName.ShouldBe("Amoxicillin");
            result.Message.ShouldBe("Medication request is successfully processed.");

            emailMock.Verify(e => e.SendEmail("ivana@example.com", "New Medication Request", "Patient Ivana Lukić requested 15 of medication Amoxicillin."), Times.Once);
        }

        private static (MedicationRequestService, Mock<IEmailSender>) CreateMedicationRequestService()
        {
            var stubRepository = CreateRepository();
            var emailMock = new Mock<IEmailSender>();

            var service = new MedicationRequestService(stubRepository, emailMock.Object);
            return (service, emailMock);
        }

        private static IMedicationRequestRepository CreateRepository()
        {

            var medicationRequests = new List<MedicationRequest>
            {
                new MedicationRequest
                {
                    Id = 1,
                    PatientName = "Ana Petrović",
                    Quantity = 10,
                    RequestDate = DateTime.UtcNow.AddDays(-5),
                    PatientEmail = "ana@example.com",
                    DoctorName = "Dr. Ilić",
                    Diagnosis = "Headache",
                    MedicationId = 1,
                    Medication =  new Medication { Id = 1, Name = "Paracetamol", Description = "Pain relief", Quantity = 0 }
                },
                new MedicationRequest
                {
                    Id = 2,
                    PatientName = "Marko Jovanović",
                    Quantity = 5,
                    RequestDate = DateTime.UtcNow.AddDays(-3),
                    PatientEmail = "marko@example.com",
                    DoctorName = "Dr. Kovač",
                    Diagnosis = "Inflammation",
                    MedicationId = 2,
                    Medication = new Medication { Id = 2, Name = "Ibuprofen", Description = "Anti-inflammatory", Quantity = 2 }
                },
                new MedicationRequest
                {
                    Id = 3,
                    PatientName = "Ivana Lukić",
                    Quantity = 15,
                    RequestDate = DateTime.UtcNow.AddDays(-2),
                    PatientEmail = "ivana@example.com",
                    DoctorName = "Dr. Janković",
                    Diagnosis = "Infection",
                    MedicationId = 3,
                    Medication = new Medication { Id = 3, Name = "Amoxicillin", Description = "Antibiotic", Quantity = 80 }
                },
                new MedicationRequest
                {
                    Id = 4,
                    PatientName = "Nikola Nikolić",
                    Quantity = 2,
                    RequestDate = DateTime.UtcNow.AddDays(-1),
                    PatientEmail = "nikola@example.com",
                    DoctorName = "Dr. Petrović",
                    Diagnosis = "Anxiety",
                    MedicationId = 4,
                    Medication = new Medication { Id = 4, Name = "Diazepam", Description = "Sedative", Quantity = 30 }
                }
            };

            var stubRepository = new Mock<IMedicationRequestRepository>();

            stubRepository.Setup(repo => repo.GetOne(It.IsAny<int>()))
                .ReturnsAsync((int id) => medicationRequests.FirstOrDefault(m => m.Id == id));

            return stubRepository.Object;

        }
    }
}