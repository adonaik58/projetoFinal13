import { API_BASE } from "../utils/baseAPI.js";

const ticketService = {
  getTicket: (filter = "", order = "") => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/ticket/get?filter=${filter}&order=${order}`, { method: "GET" })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  getTicketStory: (data) => {
    const { consumer_name = "", date_entrace = "", date_outside = "", bi = "", brand = "", model = "", code = "", order_by = "c.nome", order = "ASC", plac = "" } = data;
    return new Promise((resolve, reject) => {
      fetch(
        `${API_BASE}/ticket/get/story?consumer_name=${consumer_name}&date_entrace=${date_entrace}&date_outside=${date_outside}&bi=${bi}&brand=${brand}&model=${model}&code=${code}&order_by=${order_by}&order=${order}&plac=${plac}`,
        { method: "GET" }
      )
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
};

export default ticketService;
