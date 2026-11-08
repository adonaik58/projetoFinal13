import { API_BASE } from "../utils/baseAPI.js";

const settingsService = {
  update: (formData) => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/config/update`, {
        method: "POST",
        body: formData,
      })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  autoComplete: () => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/config/autocomplete`, {
        method: "GET",
      })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
};

export default settingsService;
