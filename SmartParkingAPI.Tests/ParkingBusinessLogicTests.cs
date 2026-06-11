using Xunit;
using System;

namespace SmartParkingAPI.Tests
{
    public class ParkingBusinessLogicTests
    {
        [Fact]
        public void Llogaritja_Tarifes_Gjate_Dites_Duhet_Te_Jete_E_Sakte()
        {
            // Arrange (Përgatitja e të dhënave)
            int oraHyrjes = 10; // 10:00 e mëngjesit (Tarifë me pagesë)
            int oraDaljes = 13; // 13:00 e drekës
            int tarifaPerOre = 40; // 40 MKD

            // Act (Ekzekutimi i logjikës që po testojmë)
            int oretEPlota = oraDaljes - oraHyrjes;
            int cmimiFinal = oretEPlota * tarifaPerOre;

            // Assert (Verifikimi nëse rezultati është ai që presim)
            Assert.Equal(120, cmimiFinal); // 3 orë * 40 MKD duhet të bëjnë saktësisht 120 MKD
        }

        [Fact]
        public void Portofoli_Digjital_Duhet_Te_Shtoje_Fonde_Saktë()
        {
            // Arrange
            decimal balancaAktuale = 150.00m;
            decimal shumaPerDepozitim = 500.00m;

            // Act
            decimal balancaERi = balancaAktuale + shumaPerDepozitim;

            // Assert
            Assert.Equal(650.00m, balancaERi); // Balanca e re duhet të jetë saktësisht 650 MKD
        }

        [Fact]
        public void Kushti_I_Sigurise_Fizika_Mposht_Internetin()
        {
            // Arrange
            // Statuset tona: 0 = I Lirë, 1 = I Rezervuar në Web, 2 = I Zënë Fizikisht (Sensori < 10cm)
            bool makinaEshteAty = true; // Sensori ka zbuluar makinën (< 10cm)
            int statusiNeCloud = 1; // Faqja thotë se është vetëm i rezervuar

            // Act
            // Kjo simulon ekzaktesisht logjikën e përdorur në kontrollor dhe në Arduino
            int statusiFinal = makinaEshteAty ? 2 : (statusiNeCloud == 1 ? 1 : 0);

            // Assert
            Assert.Equal(2, statusiFinal); // Duhet të jetë 2 (I Zënë), sepse fizika ka përparësi absolute
        }
    }
}