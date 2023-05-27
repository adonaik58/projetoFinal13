import { API_BASE } from "../utils/baseAPI.js";

const ticketService = {
  getTicket: (filter = "", order = "") => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/ticket/get?filter=${filter}&order=${order}`, { method: "GET" })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
};

export default ticketService;
