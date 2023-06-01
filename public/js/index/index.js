import graficoSemanal from "../chart/chartSemanal.js";
import graficoMensal from "../chart/chartMensal.js";
import API from "../API/API.js";

// graficoSemanal();
// graficoMensal();
const totalPayment = document.querySelector(".cards-content .card-i:nth-child(1) .item p:nth-child(2)");
const freeSpace = document.querySelector(".cards-content .card-i:nth-child(2) .item p:nth-child(2)");
const numberEntrance = document.querySelector(".cards-content .card-i:nth-child(3) .item p:nth-child(2)");
const ticketClosed = document.querySelector(".cards-content .card-i:nth-child(4) .item p:nth-child(2)");
const maxEarn = document.querySelector(".cards-content .card-i:nth-child(5) .item p:nth-child(2)");
const avgTimeStay = document.querySelector(".cards-content .card-i:nth-child(6) .item p:nth-child(2)");
// const spaceOcuped = document.querySelector(".cards-content .card-i:nth-child(7) .item p:nth-child(2)");

async function Dashboard() {
  const response = await API.dashboard.getEstatistic();

  console.log(avgTimeStay);
  totalPayment.textContent = response?.totalPayment + "kz";
  freeSpace.textContent = response?.spaceOpen;
  numberEntrance.textContent = response?.numCarEntrance;
  ticketClosed.textContent = response?.ticketClosed;
  // spaceOcuped.textContent = response?.spaceOcuped;
  maxEarn.textContent = response?.avgTimeStay + "min";
  // avgTimeStay.textContent = response?.avgTimeStay;
  // avgTimeStay.textContent = response?.maxEarn
}

Dashboard();
