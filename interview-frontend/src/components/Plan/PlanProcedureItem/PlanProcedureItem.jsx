import React, { useState, useEffect } from "react";
import ReactSelect from "react-select";
import { addUsersToPlanProcedure, getUsersByPlanAndProcedure } from "../../../api/api";
const PlanProcedureItem = ({planId, procedure, users }) => {
    const [selectedUsers, setSelectedUsers] = useState([]);

    useEffect(() => {
        const fetchSelectedUsers = async () => {
            try {
                const selectedUsers = await getUsersByPlanAndProcedure(parseInt(planId), procedure.procedureId);
                setSelectedUsers(selectedUsers.map(user => ({ value: user.userId, label: user.name })));
            } catch (error) {
                console.error("Failed to fetch selected users:", error);
            }
        };

        fetchSelectedUsers();
    }, [planId, procedure.procedureId]);

    const handleAssignUserToProcedure = async (selectedOptions) => {
        setSelectedUsers(selectedOptions);
        const userIds = selectedOptions.map(option => option.value);
        try {
            await addUsersToPlanProcedure(parseInt(planId), procedure.procedureId, userIds);
        } catch (error) {
            console.error("Failed to Add Users to the Procedure:", error);
        }
    };

    return (
        <div className="py-2">
            <div>
                {procedure.procedureTitle}
            </div>

            <ReactSelect
                className="mt-2"
                placeholder="Select User to Assign"
                isMulti={true}
                options={users}
                value={selectedUsers}
                onChange={handleAssignUserToProcedure}
            />
        </div>
    );
};

export default PlanProcedureItem;
