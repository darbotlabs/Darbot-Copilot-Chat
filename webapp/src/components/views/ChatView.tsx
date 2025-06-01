import { makeStyles, shorthands } from '@fluentui/react-components';
import { FC } from 'react';
import { useAppSelector } from '../../redux/app/hooks';
import { RootState } from '../../redux/app/store';
import { ChatWindow } from '../chat/ChatWindow';
import { ChatList } from '../chat/chat-list/ChatList';

const useClasses = makeStyles({
    container: {
        ...shorthands.overflow('hidden'),
        display: 'flex',
        flexDirection: 'row',
        alignContent: 'start',
        height: '100%',
        background: 'linear-gradient(135deg, #0A0A0F 0%, #1A0D2E 50%, #2D1B4E 100%)',
        position: 'relative',
        '&::before': {
            content: '""',
            position: 'absolute',
            top: '0',
            left: '0',
            right: '0',
            bottom: '0',
            background: 'repeating-linear-gradient(90deg, transparent, transparent 98px, rgba(138, 77, 186, 0.1) 100px)',
            pointerEvents: 'none',
        },
    },
});

export const ChatView: FC = () => {
    const classes = useClasses();
    const { selectedId } = useAppSelector((state: RootState) => state.conversations);

    return (
        <div className={classes.container}>
            <ChatList />
            {selectedId !== '' && <ChatWindow />}
        </div>
    );
};
