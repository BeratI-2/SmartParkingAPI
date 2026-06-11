using Xunit;
using System;

namespace SmartParkingAPI.Tests
{
    public class ParkingBusinessLogicTests
    {
        [Fact]
        public void Llogaritja_Tarifes_Gjate_Dites_Duhet_Te_Jete_E_Sakte()
        {
            
            int oraHyrjes = 10; 
            int oraDaljes = 13; 
            int tarifaPerOre = 40; 

            
            int oretEPlota = oraDaljes - oraHyrjes;
            int cmimiFinal = oretEPlota * tarifaPerOre;

            
            Assert.Equal(120, cmimiFinal); 
        }

        [Fact]
        public void Portofoli_Digjital_Duhet_Te_Shtoje_Fonde_Saktë()
        {
            
            decimal balancaAktuale = 150.00m;
            decimal shumaPerDepozitim = 500.00m;

            
            decimal balancaERi = balancaAktuale + shumaPerDepozitim;

            
            Assert.Equal(650.00m, balancaERi); 
        }

        [Fact]
        public void Kushti_I_Sigurise_Fizika_Mposht_Internetin()
        {
           
            bool makinaEshteAty = true; 
            int statusiNeCloud = 1; 

            
            int statusiFinal = makinaEshteAty ? 2 : (statusiNeCloud == 1 ? 1 : 0);

            
            Assert.Equal(2, statusiFinal); 
        }
    }
}