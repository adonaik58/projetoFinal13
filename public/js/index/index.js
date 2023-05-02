import graficoSemanal from "../chart/chartSemanal.js";
import graficoMensal from "../chart/chartMensal.js";
import API from "../API/API.js";

// graficoSemanal();
// graficoMensal();
async function Dashboard() {
  const response = await API.dashboard.getEstatistic();
}

Dashboard();
