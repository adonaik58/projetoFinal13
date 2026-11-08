import { API_BASE } from "../utils/baseAPI.js";

const dashboardService = {
  getEstatistic: () => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/dashboard`, { method: "GET" })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
};

export default dashboardService;
