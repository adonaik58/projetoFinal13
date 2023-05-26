import { API_BASE } from "../utils/baseAPI.js";

const ticketService = {
  getTicket: () => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/ticket/get`, { method: "GET" })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
};

export default ticketService;
