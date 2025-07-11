// Copyright (c) Microsoft. All rights reserved.

import { makeStyles } from '@fluentui/react-components';
import React from 'react';
import { IChatUser } from '../../libs/models/ChatUser';
import { useAppSelector } from '../../redux/app/hooks';
import { RootState } from '../../redux/app/store';
import { TypingIndicator } from './typing-indicator/TypingIndicator';

const useClasses = makeStyles({
    root: {
        display: 'flex',
        flexDirection: 'row',
        animationName: {
            '0%': {
                transform: 'translateY(2.4rem)',
                opacity: '0',
            },
            '100%': {
                transform: 'translateY(0)',
                opacity: '1',
            },
        },
        animationDuration: '300ms',
        animationTimingFunction: 'cubic-bezier(0.1, 0.9, 0.2, 1)',
        animationFillMode: 'forwards',
    },
});

export const ChatStatus: React.FC = () => {
    const classes = useClasses();

    const { conversations, selectedId } = useAppSelector((state: RootState) => state.conversations);
    const { users } = conversations[selectedId];
    const { activeUserInfo } = useAppSelector((state: RootState) => state.app);
    const [typingUserList, setTypingUserList] = React.useState<IChatUser[]>([]);

    React.useEffect(() => {
        const checkAreTyping = () => {
            const updatedTypingUsers: IChatUser[] = users.filter(
                (chatUser: IChatUser) => chatUser.id !== activeUserInfo?.id && chatUser.isTyping,
            );

            setTypingUserList(updatedTypingUsers);
        };
        checkAreTyping();
    }, [activeUserInfo, users]);

    let message = conversations[selectedId].botResponseStatus;
    const numberOfUsersTyping = typingUserList.length;
    if (numberOfUsersTyping === 1) {
        message = message ? `${message} and a user is typing` : 'A user is typing';
    } else if (numberOfUsersTyping > 1) {
        message = message
            ? `${message} and ${numberOfUsersTyping} users are typing`
            : `${numberOfUsersTyping} users are typing`;
    }

    if (!message) {
        return null;
    }

    return (
        <div className={classes.root}>
            <label>{message}</label>
            <TypingIndicator />
        </div>
    );
};
